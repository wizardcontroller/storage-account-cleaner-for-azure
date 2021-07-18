import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { AvailableCommand, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { from, of, ReplaySubject } from 'rxjs';
import { concatMap, map, merge, mergeMap, withLatestFrom } from 'rxjs/operators';
import { ApplianceApiService } from '../../services/appliance-api.service';

@Component({
  selector: 'app-command-palette',
  templateUrl: './command-palette.component.html',
  styleUrls: ['./command-palette.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})

@AutoUnsubscribe()
export class CommandPaletteComponent implements OnInit, OnDestroy {
  availableCommands!: Array<AvailableCommand> | null | undefined;
  availableCommandSubject = new ReplaySubject<Array<AvailableCommand>>();
  availableCommandChanges$ = this.availableCommandSubject.asObservable();

  pageModelSubject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubject.asObservable();

  commandDependencies$ = this.pageModelChanges$
                          .pipe(
                            withLatestFrom(
                              this.applianceAPiSvc.selectedStorageAccountAction$
                            ));

  commandSet$ = this.pageModelChanges$
                  .pipe(
                    concatMap(dependencyData => this.commandDependencies$)
                  )
                  .pipe(
                    map(dependencyData => {
                      const pageModel = dependencyData[0];
                      const storageAccountId = dependencyData[1];
                      const tenantId = pageModel.tenantid as string;
                      const oid = pageModel.oid as string;

                      // this is not quite right - should come from appliance context
                      const subscriptionId = pageModel.selectedSubscriptionId as string;

                      this.applianceAPiSvc.entityService
                          .workflowOperator(tenantId, oid, subscriptionId, storageAccountId)
                          .subscribe(data => {
                              this.availableCommandSubject.next(data.availableCommands as Array<AvailableCommand>  | undefined);
                          });

                    })
                  );

  // get the available commands from the workflow checkpoint
  availableCommands$ = this.applianceAPiSvc.workflowCheckpointChanges$
  .pipe
  (mergeMap(workflowCheckpoint =>
    of(workflowCheckpoint.availableCommands)
  ));

  constructor(
    private applianceAPiSvc: ApplianceApiService) { }
  ngOnDestroy(): void {
    // nothing yet

  }

  ngOnInit(): void {
    this.applianceAPiSvc.workflowCheckpointChanges$.subscribe(data => {
      this.availableCommands = data.availableCommands;
    });
  }

}

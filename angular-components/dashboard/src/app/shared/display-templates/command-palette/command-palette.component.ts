import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { AvailableCommand, OperatorPageModel } from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { from, of, ReplaySubject } from 'rxjs';
import { concatMap, map, merge, mergeMap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
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

  commandDependencies$ = this.applianceAPiSvc.selectedStorageAccountAction$
                          .pipe(
                            withLatestFrom(
                              this.pageModelChanges$
                            ));
/*
  commandSet$ = this.pageModelChanges$
                  .pipe(
                    concatMap(dependencyData => this.commandDependencies$)
                  )
*/

  dataSet$ = this.commandDependencies$

                  .pipe(
                    map(dependencyData => {

                      console.log("command palette dependencies available");
                      const pageModel = dependencyData[1];
                      const storageAccountId = dependencyData[0];
                      const tenantId = pageModel.tenantid as string;
                      const oid = pageModel.oid as string;

                      // this is not quite right - should come from appliance context
                      const subscriptionId = pageModel.selectedSubscriptionId as string;

                      this.applianceAPiSvc.entityService
                        .getWorkflowCheckpoint(tenantId, oid, subscriptionId)
                        .subscribe(data => {
                          console.log("command palette has workflow checkpoint");
                          console.log("command palette: command timestamp is " + data.message);
                              this.availableCommandSubject.next(data.availableCommands as Array<AvailableCommand>  | undefined);
                          })
                          ;

                    })
                  )
                  .subscribe(x =>{
                    console.log("dependencies data available");
                  });

  // get the available commands from the workflow checkpoint
  availableCommands$ = this.applianceAPiSvc.workflowCheckpointChanges$
  .pipe
  (mergeMap(workflowCheckpoint =>
    of(workflowCheckpoint.availableCommands)
  ));

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService) { }
  ngOnDestroy(): void {
    // nothing yet

  }

  ngOnInit(): void {

    console.log("command palette onInit");

    this.apiConfigSvc.operatorPageModelChanges$.subscribe(pageModel => {
      console.log("command palette view has page model");
      // metrics retention component has operator page model
      this.pageModelSubject.next(pageModel);
    });

    this.commandDependencies$.subscribe(dependencies => {
      console.log("command dependencies available: storage account Id " + dependencies[1] );

    })
    ;

    this.applianceAPiSvc.selectedStorageAccountAction$.subscribe(newStorageAcct => {
      console.log("command palette has selected storage account" + newStorageAcct);
    });


  }



}

import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import {
  AvailableCommand,
  OperatorPageModel,
  WorkflowOperationCommand,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { from, of, Operator, ReplaySubject } from 'rxjs';
import {
  concatMap,
  map,
  merge,
  mergeMap,
  withLatestFrom,
} from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { WorkflowOperationCommandImpl } from '../../models/WorkflowOperationCommandImpl';
@Component({
  selector: 'app-command-palette',
  templateUrl: './command-palette.component.html',
  styleUrls: ['./command-palette.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
@AutoUnsubscribe()
export class CommandPaletteComponent implements OnInit, OnDestroy {
  availableCommands!: Array<AvailableCommand> | null | undefined;
  availableCommandSubject = new ReplaySubject<Array<AvailableCommand>>();
  availableCommandChanges$ = this.availableCommandSubject.asObservable();
  hasSelectedCommand = false;

  selectedCommand!: AvailableCommand;

  currentPageModel!: OperatorPageModel;

  selectedStorageAccountId!: string;

  pageModelSubject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubject.asObservable();

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService
  ) {
    this.selectedCommand = { menuLabel: 'select a command' };
  }

  submitCommand(command: AvailableCommand): void {
    console.log('submitting command: ' + command.menuLabel);
    const oid = this.currentPageModel.oid as string;
    const tenantId = this.currentPageModel.tenantid as string;
    const subscriptionId = this.currentPageModel
      .selectedSubscriptionId as string;
    const storageAccountId = this.selectedStorageAccountId as string;
    const workflowOperationCommand = new WorkflowOperationCommandImpl();
    workflowOperationCommand.candidateCommand = this.selectedCommand;
    // workflowOperationCommand.timeStamp = Date.UTC.toString();
    // workflowOperationCommand.displayMessage = this.selectedCommand.worklowOperationDisplayMessage;
    workflowOperationCommand.commandCode =
      this.selectedCommand.workflowOperation;

      workflowOperationCommand.timeStamp = Date.UTC.toString();

    this.applianceAPiSvc.entityService
      .workflowOperator(
        tenantId,
        oid,
        subscriptionId,
        storageAccountId,
        workflowOperationCommand
      )
      .pipe(
        map((result) => {
          console.log('workflow operator results available');
          this.availableCommandSubject.next(
            result.availableCommands as Array<AvailableCommand>
          );

          this.applianceAPiSvc.ensurePageModelSubject();

          // update dependencies
          this.applianceAPiSvc.ensureApplianceSessionContextSubject(
            tenantId,
            subscriptionId,
            oid
          );
        })
      )
      .subscribe(
        (placeholder) => {
          console.log('');
        },
        (errors) => {
          console.log(JSON.stringify(errors));
        }
      );
  }

  onSelect(command: AvailableCommand): void {
    // nothing yet
    this.selectedCommand = command;
    this.hasSelectedCommand = true;
    console.log('command selected: ' + command.menuLabel);
  }

  ngOnDestroy(): void {
    // nothing yet
  }

  ngOnInit(): void {
    console.log('command palette onInit');

    this.apiConfigSvc.operatorPageModelChanges$.subscribe((pageModel) => {
      console.log('command palette view has page model');
      // metrics retention component has operator page model
      this.pageModelSubject.next(pageModel);
      this.currentPageModel = pageModel;
    });

    this.applianceAPiSvc.workflowCheckpointChanges$.subscribe(
      (workflowCheckpoint) => {
        this.availableCommandSubject.next(
          workflowCheckpoint.availableCommands as Array<AvailableCommand>
        );
      }
    );
  }
}

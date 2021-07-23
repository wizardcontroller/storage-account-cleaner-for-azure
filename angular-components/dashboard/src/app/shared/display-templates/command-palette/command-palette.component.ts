import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import {
  AvailableCommand,
  OperatorPageModel,
  WorkflowCheckpointDTO,
  WorkflowOperationCommand,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { from, interval, Observable, of, Operator, ReplaySubject, Subject } from 'rxjs';
import {
  catchError,
  concatMap,
  debounce,
  map,
  merge,
  mergeMap,
  publishReplay,
  refCount,
  share,
  shareReplay,
  tap,
  withLatestFrom, debounceTime
} from 'rxjs/operators';
import { ApiConfigService } from 'src/app/core/ApiConfig.service';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { WorkflowOperationCommandImpl } from '../../models/WorkflowOperationCommandImpl';
import { DatePipe } from '@angular/common';
import { MessageService } from 'primeng/api';
import { ToastMessage } from '../../../models/ToastMessage';

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


  isRefreshing: boolean = false;
  isRefreshingSource = new Subject<boolean>();
  isRefreshingChanges$ = this.isRefreshingSource.asObservable();

  isShowSpinnerSource = new Subject<boolean>();
  isShowSpinnerChanges$ = this.isShowSpinnerSource.asObservable();

  isRefreshingPipe = this.applianceAPiSvc.isRefreshingChanges$
    .pipe
    (
      tap(tapped => {
        console.log("command palette is refreshing = " + tapped);
      }),
      map(isRefreshing => {
        this.isRefreshing = isRefreshing;
        this.isRefreshingSource.next(isRefreshing);

        if (isRefreshing) {
          console.log("showing refresh toast");
          const toast = new ToastMessage();
          toast.detail = "checking the appliance state";
          toast.summary = "command palette Updating ";
          toast.sticky = false;
          toast.life = 1000 * 8;
          toast.severity = "info";
          this.showToast(toast);

        }


      })

    )

    .subscribe(data => { }, error => {
      const toast = new ToastMessage();
      toast.detail = JSON.stringify(error);
      toast.summary = "Error";
      toast.sticky = false;
      toast.life = 1000 * 8;
      toast.severity = "error";
      this.showToast(toast);
      this.isRefreshing = false; this.isRefreshingSource.next(false);
    });

  workflowCheckpoint!: WorkflowCheckpointDTO;

  selectedCommand!: AvailableCommand;

  currentPageModel!: OperatorPageModel;

  selectedStorageAccountId!: string;

  pageModelSubject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubject.asObservable();

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService,
    private messageService: MessageService
  ) {
    this.selectedCommand = { menuLabel: 'select a command' };
  }

  showToast(message: ToastMessage): void {
    this.messageService.add(message);
  }

  submitCommand(command: AvailableCommand): void {
    console.log('submitting command: ' + command.menuLabel);
    const toast = new ToastMessage();
    toast.detail = command.worklowOperationDisplayMessage as string;
    toast.summary = command.menuLabel as string;

    toast.severity = "info";
    this.showToast(toast);

    // this.applianceAPiSvc.isRefreshingSource.next(true);
    this.isShowSpinnerSource.next(true);
    this.isRefreshing = true;
    const oid = this.currentPageModel.oid as string;
    const tenantId = this.currentPageModel.tenantid as string;
    const subscriptionId = this.currentPageModel
      .selectedSubscriptionId as string;
    const storageAccountId = this.selectedStorageAccountId as string;
    const workflowOperationCommand = new WorkflowOperationCommandImpl();
    const datepipe: DatePipe = new DatePipe('en-US')
    let formattedDate = datepipe.transform(new Date(), 'YYYY-mm-dd HH:mm:ss')

    workflowOperationCommand.candidateCommand = this.selectedCommand;
    // workflowOperationCommand.timeStamp = Date.UTC.toString();
    // workflowOperationCommand.displayMessage = this.selectedCommand.worklowOperationDisplayMessage;
    workflowOperationCommand.commandCode =
      this.selectedCommand.workflowOperation;

    // a copout cos we need a c# compatible formatted date
    workflowOperationCommand.timeStamp = this.workflowCheckpoint.timeStamp;

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

          // this.applianceAPiSvc.isRefreshingSource.next(false);
          this.isShowSpinnerSource.next(false);
          this.isRefreshing = false;
        }),
        catchError(err => {
          const toast = new ToastMessage();
          toast.detail = "failed to submit command to the appliance";
          toast.summary = "update failed";
          toast.sticky = true;
          toast.life = 1000 * 8;
          toast.severity = "error";
          this.applianceAPiSvc.isRefreshingSource.next(false);
          this.isShowSpinnerSource.next(false);
          this.isRefreshing = false;
          this.showToast(toast);
          return of([]);
        })
      )
      .subscribe(
        (placeholder) => {
          console.log('');

        },
        (errors) => {
          console.log(JSON.stringify(errors));
          console.log("error submitting");

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

    //this.apiConfigSvc.operatorPageModelChanges$.subscribe((pageModel) => {
    //  console.log('command palette view has page model');
    //  // metrics retention component has operator page model
    //  this.pageModelSubject.next(pageModel);
    //  this.currentPageModel = pageModel;
    //});

    this.apiConfigSvc.operatorPageModelChanges$.
      pipe(
        map(changes => {
          this.pageModelSubject.next(changes);
          this.currentPageModel = changes;
        })
      ).subscribe();

    this.applianceAPiSvc.workflowCheckpointChanges$.
      pipe(
        map(workflowCheckpoint => {

          this.workflowCheckpoint = workflowCheckpoint;
          this.availableCommandSubject.next(
            workflowCheckpoint.availableCommands as Array<AvailableCommand>
          );
        }))
      .subscribe();


  }
}

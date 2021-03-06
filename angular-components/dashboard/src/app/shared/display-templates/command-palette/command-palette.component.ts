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
import {
  combineLatest,
  from,
  interval,
  Observable,
  of,
  Operator,
  ReplaySubject,
  Subject,
  timer,
} from 'rxjs';
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
  withLatestFrom,
  debounceTime,
  filter,
  publish,
  distinctUntilChanged,
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
  availableCommandSubject = new Subject<Array<AvailableCommand>>();
  availableCommandChanges$ = this.availableCommandSubject.asObservable();
  hasSelectedCommand = false;

  isMousingOver = true;

  isRefreshing: boolean = false;

  isShowSpinnerSource = new ReplaySubject<boolean>();
  isShowSpinnerChanges$ = this.isShowSpinnerSource.asObservable();

  refreshTimer$ = timer(0, 1000 * 30);

  isRefreshingPipe$ = combineLatest(
    this.refreshTimer$,
    this.applianceAPiSvc.workflowCheckpointChanges$
  )
    .pipe(
      distinctUntilChanged(),
      // filter(f => this.workflowCheckpoint.timeStamp?.match(this.workflowCheckpoint.timeStamp) === null),
      // filter(f => f[1].timeStamp === this.workflowCheckpoint.timeStamp),
      tap((tapped) => {
        console.log('command palette is refreshing = ' + tapped);
      }),
      map(([isRefreshing, data]) => {
        this.isRefreshing = true;
        // this.isRefreshingSource.next(isRefreshing);

        // if (isRefreshing) {
        console.log('showing refresh toast');
        const toast = new ToastMessage();
        toast.detail = 'checking the appliance state ';
        toast.summary = 'command palette Updating ';
        toast.sticky = false;
        toast.life = 1000 * 8;
        toast.severity = 'warning';
        // this.showToast(toast);

        // this.getPagemodelChangesPipe().subscribe();
        this.getWorkflowCheckpointChangesPipe().subscribe();
        //}
        this.isRefreshing = false;
      })
    )
    .subscribe();

  isRefreshingPipe = this.applianceAPiSvc.isRefreshingChanges$.pipe(
    distinctUntilChanged(),
    tap((tapped) => {
      console.log('command palette is refreshing = ' + tapped);
    }),
    map((isRefreshing) => {
      this.isRefreshing = isRefreshing;
      // this.isRefreshingSource.next(isRefreshing);

      // if (isRefreshing) {
      console.log('showing refresh toast');
      const toast = new ToastMessage();
      toast.detail = 'checking the appliance state';
      toast.summary = 'command palette Updating ';
      toast.sticky = false;
      toast.life = 1000 * 8;
      toast.severity = 'info';
      // this.showToast(toast);

      this.getPagemodelChangesPipe().subscribe();
      this.getWorkflowCheckpointChangesPipe().subscribe();
      //}
    })
  );

  workflowCheckpoint!: WorkflowCheckpointDTO;

  selectedCommand!: AvailableCommand;

  currentPageModel!: OperatorPageModel;

  selectedStorageAccountId!: string;

  pageModelSubject = new Subject<OperatorPageModel>();
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
    const oid = this.currentPageModel.oid as string;
    const tenantId = this.currentPageModel.tenantid as string;
    const subscriptionId = this.currentPageModel
      .selectedSubscriptionId as string;
    const storageAccountId = this.selectedStorageAccountId as string;
    const workflowOperationCommand = new WorkflowOperationCommandImpl();
    const datepipe: DatePipe = new DatePipe('en-US');

    const toast = new ToastMessage();
    toast.detail = `${command.worklowOperationDisplayMessage}: oid=${oid}`;
    toast.summary = command.menuLabel as string;

    toast.severity = 'info';
    this.showToast(toast);

    // this.applianceAPiSvc.isRefreshingSource.next(true);
    this.isShowSpinnerSource.next(true);
    this.isRefreshing = true;

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

          this.isMousingOver = true;
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

        catchError((err) => {
          console.log('catchError(): error submitting command');
          const toast = new ToastMessage();
          toast.detail = 'failed to submit command to the appliance';
          toast.summary = 'update failed';
          toast.sticky = true;
          toast.life = 1000 * 8;
          toast.severity = 'error';

          this.isShowSpinnerSource.next(false);
          this.isRefreshing = false;
          this.showToast(toast);

          this.isMousingOver = true;
          return of([]);
        })
      )
      .subscribe((placeholder) => {
        console.log('');
      });
  }

  focusOut() : void{
    this.isMousingOver = true;
  }
  mouseOver(command: AvailableCommand): void {
    if (this.isMousingOver) {
      this.selectedCommand = command;
      this.hasSelectedCommand = true;
    }
  }

  onSelect(command: AvailableCommand): void {
    // nothing yet
    this.selectedCommand = command;
    this.hasSelectedCommand = true;
    this.isMousingOver = false;
  }

  ngOnDestroy(): void {
    // nothing yet
  }

  getPagemodelChangesPipe(): Observable<OperatorPageModel> {
    return this.apiConfigSvc.operatorPageModelChanges$.pipe(
      distinctUntilChanged(),
      map((changes) => {
        this.pageModelSubject.next(changes);
        this.currentPageModel = changes;
        return changes;
      }),
      tap((t) => {
        const toast = new ToastMessage();
        toast.detail = 'operator pagemodel is available';
        toast.summary = 'updating';
        toast.sticky = false;
        toast.life = 1000 * 8;
        toast.severity = 'info';
        // this.showToast(toast);
      })
    );
  }

  getWorkflowCheckpointChangesPipe() {
    console.log('getWorkflowCheckpointChangesPipe()');
    return this.applianceAPiSvc.workflowCheckpointChanges$.pipe(
      distinctUntilChanged(),

      tap((t) => {
        const toast = new ToastMessage();
        toast.detail = 'checking the appliance workflow checkpoint';
        toast.summary = 'workflow checkpoint updating ';
        toast.sticky = false;
        toast.life = 1000 * 8;
        toast.severity = 'info';
        // this.showToast(toast);
      }),
      map((workflowCheckpoint) => {
        // let difference = this.workflowCheckpoint.availableCommands?.filter(f => !workflowCheckpoint.availableCommands?.includes(f));
        console.log(
          `null workflow checkpoint? ${
            this.workflowCheckpoint === null
          }`
        );

        this.workflowCheckpoint = workflowCheckpoint;
        this.availableCommandSubject.next(
          workflowCheckpoint.availableCommands as Array<AvailableCommand>
        );
      })
    );
  }

  ngOnInit(): void {
    console.log('command palette onInit');
    this.getPagemodelChangesPipe().subscribe();
    this.getWorkflowCheckpointChangesPipe().subscribe();
  }
}

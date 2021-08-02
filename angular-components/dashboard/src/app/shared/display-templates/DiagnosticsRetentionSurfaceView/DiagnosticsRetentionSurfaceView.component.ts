import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { ActivatedRoute } from '@angular/router';
import {
  DiagnosticsRetentionSurfaceItemEntity,
  OperatorPageModel,
  StorageAccountDTO,
  TableStorageRetentionPolicy,
} from '@wizardcontroller/sac-appliance-lib';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { combineLatest, ReplaySubject } from 'rxjs';
import { concatMap, map, mergeMap, tap, withLatestFrom } from 'rxjs/operators';
import { ApiConfigService } from '../../../core/ApiConfig.service';
import { ICanBeHiddenFromDisplay } from '../../interfaces/ICanBeHiddenFromDisplay';
import { ApplianceApiService } from '../../services/appliance-api.service';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { PrimeIcons } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { ToggleButton } from 'primeng/togglebutton';

import { FullCalendarModule } from 'primeng/fullcalendar';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { RetentionPeriodForFullCalendarPipe } from '../../pipes/retention-Period-For-FullCalendar.pipe';
@Component({
  selector: 'app-DiagnosticsRetentionSurfaceView',
  templateUrl: './DiagnosticsRetentionSurfaceView.component.html',
  styleUrls: ['./DiagnosticsRetentionSurfaceView.component.css'],
})
@AutoUnsubscribe()
export class DiagnosticsRetentionSurfaceViewComponent
  implements OnDestroy, OnInit, ICanBeHiddenFromDisplay
{
  @ViewChild('dv') dv!: DataView;
  @ViewChild('filterItemsBtn') filterItemsBtn!: ToggleButton;

  showOnlyExistingItems = false;
  isSideNavOpen = false;
  rangeDates!: Array<Date>;

  events!: any[];
  header!: any;
  options!: any;

  private pageModelSubuject = new ReplaySubject<OperatorPageModel>();
  pageModelChanges$ = this.pageModelSubuject.asObservable();


  curentStorageAccount!: StorageAccountDTO;
  currentStorageAccount$ =
    this.applianceAPiSvc.currentStorageAccountChanges$.subscribe((data) => {
      const newData = data;
      if (newData) {
        this.curentStorageAccount = newData;
        console.log(
          `diagnosticsCurrentStorageAccount$ updated with storage account ${this.curentStorageAccount.name}`
        );
      }
    });


  diagnosticEntitiesPipe$ = combineLatest([
    this.pageModelChanges$,
    this.applianceAPiSvc.currentStorageAccountChanges$,
    this.applianceAPiSvc.workflowCheckpointTimer$
  ])
    .pipe(
      tap((t) => {
        console.log('diagnosticEntitiesPipe$ updated with dependencies');
      }),
      map((dependencyData) => {
        var pageModel = dependencyData[0];
        var storageAccountId = dependencyData[1];

        this.applianceAPiSvc.entityService
          .getRetentionPolicyForStorageAccount(
            pageModel.tenantid as string,
            pageModel.oid as string,
            pageModel.selectedSubscriptionId as string,
            storageAccountId.id as string
          )
          .pipe(tap((t) => {
            console.log(`diagnostics has updated retention surface for ${storageAccountId.id}`);
          }))
          .subscribe((data: TableStorageRetentionPolicy) => {
            this.diagnosticsRetentionSurfaceEntitySource.next(
              data?.tableStorageEntityRetentionPolicy
                ?.diagnosticsRetentionSurface
                ?.diagnosticsRetentionSurfaceEntities as DiagnosticsRetentionSurfaceItemEntity[]
            );
            // console.log(`diagnosticentities$ updated storage account ${storageAccountId}`)
          });
      })
    )
    .subscribe();

  private diagnosticsRetentionSurfaceEntitySource = new ReplaySubject<
    DiagnosticsRetentionSurfaceItemEntity[]
  >();

  diagnosticsRetentionSurfaceEntityChanges$ =
    this.diagnosticsRetentionSurfaceEntitySource.asObservable();

  pageModelPipe = this.apiConfigSvc.operatorPageModelChanges$.pipe(
    map((pageModel) => {
      console.log('diagnostics retention view has updated page model');
      // metrics retention component has operator page model
      this.pageModelSubuject.next(pageModel);
    })
  );

  constructor(
    private apiConfigSvc: ApiConfigService,
    private applianceAPiSvc: ApplianceApiService
  ) {
    this.isShow = false;
  }

  ngOnDestroy(): void {
    // nothing yet
  }

  isShow: boolean;
  toggleDisplay(): void {}

  ngOnInit() {
    this.pageModelPipe.subscribe();


    this.options = {
      plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

      header: {
        left: 'prev,next',
        center: 'title',
        right: 'dayGridMonth,timeGridWeek,timeGridDay',
      },
      editable: true,
      dayMaxEvents: true,
    };
  }

  toggleSideNav(): void {
    console.log('toggling sidenav');
    this.isSideNavOpen = !this.isSideNavOpen;
  }

  filterChanged(e: boolean) {
    const filterExpression = e ? 'true' : 'false';

    this.dv.filter(filterExpression);
    console.log(`filter expression ${filterExpression}`);
    // this.filterItemsBtn.checked = e.returnValue;
  }
}

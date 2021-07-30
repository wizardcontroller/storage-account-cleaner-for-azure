import { DatePipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';
import { DiagnosticsRetentionSurfaceItemEntity } from '@wizardcontroller/sac-appliance-lib';
import { FullCalendarEvent } from '../models/primeng/FullCalendarEvent';

@Pipe({
  name: 'retentionPeriodForFullCalendar'
})
export class RetentionPeriodForFullCalendarPipe implements PipeTransform {

  constructor(){

  }

  transform(value: DiagnosticsRetentionSurfaceItemEntity, args?: any): Array<FullCalendarEvent> {
    const ret = new Array<FullCalendarEvent>();
    const datepipe: DatePipe = new DatePipe('en-US');
    const instance = new FullCalendarEvent();
    Object.assign(instance, value);


    instance.start =  new Date(value.entityTimestampLowWatermark as string);
    instance.end = new Date(value.entityTimestampHighWatermark as string);
    instance.title = value.tableName as string;
    ret.push(instance);

    return ret;
  }

}

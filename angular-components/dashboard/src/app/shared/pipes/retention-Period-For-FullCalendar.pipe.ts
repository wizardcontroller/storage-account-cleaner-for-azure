import { Pipe, PipeTransform } from '@angular/core';
import { DiagnosticsRetentionSurfaceItemEntity } from '@wizardcontroller/sac-appliance-lib';
import { FullCalendarEvent } from '../models/primeng/FullCalendarEvent';

@Pipe({
  name: 'retention-Period-For-FullCalendar'
})
export class RetentionPeriodForFullCalendarPipe implements PipeTransform {

  transform(value: DiagnosticsRetentionSurfaceItemEntity, args?: any): Array<FullCalendarEvent> {
    return null;
  }

}

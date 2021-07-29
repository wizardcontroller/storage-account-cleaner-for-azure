import { ValueConverter } from '@angular/compiler/src/render3/view/template';
import { Pipe, PipeTransform } from '@angular/core';
import { MetricsRetentionSurfaceItemEntity } from '@wizardcontroller/sac-appliance-lib';
import { RetentionSurfaceItemTimeline } from '../models/primeng/IRetentionSurfaceItemTimeline';
import { TimelineEvent } from '../models/primeng/TimelineEvent';
@Pipe({
  name: 'datesToTimeLineEventsPipe'
})

export class DatesToTimeLineEventsPipePipe implements PipeTransform {
  transform(value: RetentionSurfaceItemTimeline): Array<TimelineEvent> {
      const ret = new Array<TimelineEvent>();

      const instance = new TimelineEvent();
      Object.assign(instance, value);

      instance.itemDescription = "Least Recent Timestamp"
      instance.timestamp = value.entityTimestampLowWatermark;

      ret.push(instance);

      const instance2 = new TimelineEvent();
      Object.assign(instance2, value);
      instance2.itemDescription = "Most Recent Timestamp"
      instance2.timestamp = value.entityTimestampHighWatermark;
      ret.push(instance2);

      return ret;
    }
}

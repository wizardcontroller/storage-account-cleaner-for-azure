import { RetentionSurfaceItemTimeline } from './IRetentionSurfaceItemTimeline';

export class TimelineEvent implements RetentionSurfaceItemTimeline {

  mostRecentEntityTimestamp?: string;
  leastRecentEntityTimestamp?: string;
  entityTimestampLowWatermark?: string;
  entityTimestampHighWatermark?: string;
  itemDescription?: string | null;
  itemExists?: boolean;
  tableName?: string | null;
  documentationLink?: string | null;
  policyAgeTriggerInMonths?: number;
  timestamp?: string;
}

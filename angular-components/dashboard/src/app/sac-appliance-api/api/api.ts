export * from './config.service';
import { ConfigService } from './config.service';
export * from './dashboardApi.service';
import { DashboardApiService } from './dashboardApi.service';
export * from './retentionEntities.service';
import { RetentionEntitiesService } from './retentionEntities.service';
export const APIS = [ConfigService, DashboardApiService, RetentionEntitiesService];

using AutoMapper;
using com.ataxlab.azure.table.retention.models.models;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.automapper

{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{

			// CreateMap<Microsoft.Azure.Management.Storage.Fluent.PublicEndpoints, models.PublicEndpointsDto>();

			// CreateMap<models.EndpointsDto, Microsoft.Azure.Management.Storage.Fluent.Models.Endpoints>();

			CreateMap<Microsoft.Azure.Management.Storage.Fluent.PublicEndpoints, PublicEndpointsDto>();
			CreateMap<Microsoft.Azure.Management.Storage.Fluent.Models.Endpoints, EndpointsDto>();
			CreateMap<DurableOrchestrationStatus, DurableOrchestrationStateDTO>();
			// Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
		}
	}
}

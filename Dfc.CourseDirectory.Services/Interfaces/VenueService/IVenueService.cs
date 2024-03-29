﻿using Dfc.CourseDirectory.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Venues;
using Dfc.CourseDirectory.Models.Models.Venues;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueService
    {
        Task<IResult<IVenue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria);

        Task<IResult<IVenue>> GetVenueByVenueIdAsync(IGetVenueByVenueIdCriteria criteria);

        Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);

        Task<IResult<IVenue>> AddAsync(IVenue venue);

        Task<IResult<IVenue>> UpdateAsync(IVenue venue);

        Task<Result<List<Venue>>> GetAllVenues();
    }
}

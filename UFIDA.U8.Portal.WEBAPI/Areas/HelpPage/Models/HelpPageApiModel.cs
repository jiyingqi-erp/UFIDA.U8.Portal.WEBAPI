using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Web.Http.Description;

namespace UFIDA.U8.Portal.WEBAPI.Areas.HelpPage.Models
{
    /// <summary>
    /// The model that represents an API displayed on the help page.
    /// </summary>
    public class HelpPageApiModel
    {
        public ApiDescription ApiDescription { get; set; }

        public IDictionary<MediaTypeHeaderValue, object> SampleRequests { get; private set; }

        public IDictionary<MediaTypeHeaderValue, object> SampleResponses { get; private set; }

        public Collection<string> ErrorMessages { get; private set; }

        public HelpPageApiModel()
        {
            SampleRequests = new Dictionary<MediaTypeHeaderValue, object>();
            SampleResponses = new Dictionary<MediaTypeHeaderValue, object>();
            ErrorMessages = new Collection<string>();
        }
    }
}
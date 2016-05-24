using System.Web.Http;

namespace SampleWebsite.Models.ApiControllers
{
    public class PersonController : ApiController
    {
        [HttpPost]
        public Person GetPerson()
        {
            return new Person { Name = "Fred Flintstone", Gender = Gender.Male };
        }
    }
}
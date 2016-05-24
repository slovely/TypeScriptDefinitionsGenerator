using Microsoft.AspNet.SignalR;

namespace SampleWebsite.Models.Hubs
{
    public class PersonHub : Hub<IPersonHubClient>
    {
        public void CreatePerson(Person person)
        {
            // Save the person or whatever
            // Notify other users that a new person has been created
            Clients.All.PersonCreated(person);
        }
    }

    public interface IPersonHubClient
    {
        /// <summary>
        /// Notify clients when a new person has been created
        /// </summary>
        void PersonCreated(Person person);
    }
}
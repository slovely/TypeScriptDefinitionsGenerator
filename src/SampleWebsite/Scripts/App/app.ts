$(() => {

    // Simulate getting a person
    Api.Person.getPerson()
        .done(result => {
            console.log("Name: " + result.Name + ", Gender enum value: " + result.Gender);
            console.log("   (Gender string value is " + SampleWebsite.Models.Gender[result.Gender] + ")");
            console.dir(result);
        });


    // Create client-side implementation for PersonHub
    const personHubInstance = new PersonHub();

    // Start the SignalR connection
    $.connection.hub.start()
        .done(_ => {

            // Simulate calling a server-side SignalR method
            var hub = $.connection.timeHub;
            hub.server.currentServerTime()
                .done(result => {
                    console.log("The server time is: " + result);
                });

            // Create a person via signalr... each connected client should get notified
            var personHub = $.connection.personHub;
            personHub.server.createPerson({ Name: "Wonder Woman", Gender: SampleWebsite.Models.Gender.Female });

        });
});
$(() => {

    Api.Person.getPerson()
        .done(result => {
            console.log("Name: " + result.Name + ", Gender enum value: " + result.Gender);
            console.log("   (Gender string value is " + SampleWebsite.Models.Gender[result.Gender] + ")");
            console.dir(result);
        });

})
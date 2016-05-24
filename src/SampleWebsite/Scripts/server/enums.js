var SampleWebsite;
(function (SampleWebsite) {
    var Models;
    (function (Models) {
        (function (Gender) {
            Gender[Gender["Male"] = 0] = "Male";
            Gender[Gender["Female"] = 1] = "Female";
            Gender[Gender["None"] = 2] = "None";
            Gender[Gender["PreferNotToSay"] = 3] = "PreferNotToSay";
            Gender[Gender["Other"] = 4] = "Other";
        })(Models.Gender || (Models.Gender = {}));
        var Gender = Models.Gender;
    })(Models = SampleWebsite.Models || (SampleWebsite.Models = {}));
})(SampleWebsite || (SampleWebsite = {}));
//# sourceMappingURL=enums.js.map
using System;
using System.Collections.Generic;
using SampleWebsite.Models.OtherModelsWeWantToConvert.SubFolder;

namespace SampleWebsite.Models.OtherModelsWeWantToConvert
{
    public class WillBeConvertedEvenThoughNotUsed
    {
        public Guid Id { get; set; }
        public IEnumerable<string> Info { get; set; }
        public GenericType<Person> SampleGenericTypeProperty { get; set; }
    }
}
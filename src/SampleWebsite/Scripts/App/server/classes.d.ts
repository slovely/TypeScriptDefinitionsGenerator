
declare module SampleWebsite.Models {
	interface Person {
		Gender: SampleWebsite.Models.Gender;
		Name: string;
	}
}
declare module SampleWebsite.Models.OtherModelsWeWantToConvert {
	interface WillBeConvertedEvenThoughNotUsed {
		Id: string;
		Info: string[];
		SampleGenericTypeProperty: SampleWebsite.Models.OtherModelsWeWantToConvert.SubFolder.GenericType<SampleWebsite.Models.Person>;
	}
}
declare module SampleWebsite.Models.OtherModelsWeWantToConvert.SubFolder {
	interface GenericType<T> {
		Property: T;
	}
}
declare module SampleWebsite.MoreModels {
	interface SecondaryModelLibraray {
		StringProperty: string;
	}
}
declare module SampleWebsite.MoreModels.Nested {
	interface NestedModel {
		Id: string;
	}
}

namespace DocumentsFillerAPI.Structures
{
	//public class TeacherStruct
	//{
	//	public Guid ID { get; set; }
	//	public string FirstName { get; set; }
	//	public string SecondName { get; set; }
	//	public string Patronymic { get; set; }
	//}

	public class TeacherFullInfoStruct
	{
		public Guid ID { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public AcademicTitleStruct AcademicTitle { get; set; }
	}
}

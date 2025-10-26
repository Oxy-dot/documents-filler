namespace DocumentsFillerAPI.Structures
{
	public class TeacherStruct
	{
		public Guid ID { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public string Patronymic { get; set; }
		public Guid MainBetID { get; set; }
		public Guid SecondBetID { get; set; }
		public Guid ExcessiveBetID { get; set; }
	}
}

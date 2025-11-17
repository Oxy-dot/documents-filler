namespace DocumentsFillerAPI.Structures
{
	public record struct AcademicTitleStruct
	{
		public Guid ID { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
	}
}

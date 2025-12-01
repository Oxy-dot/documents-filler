namespace DocumentsFillerAPI.Structures
{
	public class BetStruct
	{
		public Guid ID { get; set; }
		public double BetAmount { get; set; }
		public int HoursAmount { get; set; }
		public Guid TeacherID { get; set; }
		public Guid DepartmentID { get; set; }
		public bool IsAdditional { get; set; }
	}
}

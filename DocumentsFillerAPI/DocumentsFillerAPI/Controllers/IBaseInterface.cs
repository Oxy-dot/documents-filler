namespace DocumentsFillerAPI.Controllers
{
	public interface IBaseInterface
	{
		public ResultMessage Update();
		public ResultMessage Delete();
		public void Insert();
		//public ResultMessage Search();
	}
}

using DocumentsFillerAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class filesController : ControllerBase
	{
		private FilePostgreProvider _provider = new FilePostgreProvider();

		[HttpGet("get")]
		public async Task<IActionResult> GetFiles(uint count, uint startIndex)
		{
			var files = await _provider.List(count, startIndex);

			var filesJson = JsonSerializer.Serialize(files.Files);
			var jsonResult = new JsonObject()
			{
				["message"] = files.Message.Message,
				["files"] = JsonNode.Parse(filesJson)!.AsArray()
			};
			return Ok(jsonResult);
		}

		[HttpPost("insert")]
		public async Task<IActionResult> InsertFiles()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var filesToInsert = jBody?["insert"]?.AsArray()?.Select(a => new FileStruct
				{
					FileName = (string)a["fileName"]!,
					FileType = (Guid)a["fileType"]!,
					CreationDate = DateTime.UtcNow,
					Content = a["content"]!.AsObject(),
				}).ToList() ?? new List<FileStruct>();

				if (filesToInsert.Count == 0)
					throw new Exception("Files to insert were empty");

				var insertFilesResult = await _provider.InsertFiles(filesToInsert);
				//var filesJson = JsonSerializer.Serialize(insertFilesResult);

				var jsonResult = new JsonObject()
				{
					["message"] = insertFilesResult.Message.Message,
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.InsertedFiles))!.AsArray(),
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.NotInsertedFiles))!.AsArray(),
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<MinimalFileInfoStruct>()))!.AsArray(), 
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(), 
				};

				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteFiles()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var filesToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a!)?.ToList() ?? new List<Guid>();

				if (filesToDelete.Count == 0)
					throw new Exception("Files to delete were empty");

				var deleteFilesResult = await _provider.DeleteFiles(filesToDelete);

				var jsonResult = new JsonObject()
				{
					["message"] = deleteFilesResult.Message.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(deleteFilesResult.DeleteResults.Select(a => new { Message = a.Message, a.IsSuccess, a.FileID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<DeleteFilesStruct>())),
				};

				return BadRequest(jsonResult);
			}
		}
	}
}

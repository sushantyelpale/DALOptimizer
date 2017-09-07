using System;
using System.Collections.Generic;

namespace DALOptimizer
{
	/// <summary>
	/// This class contains various IndexOf calls.
	/// Only the ones in the first should be detected as missing a StringComparison.
	/// </summary>
	public class Test
	{
		public void StringIndexOfStringCalls(List<string> list)
		{
			list[0].IndexOf(".com", StringComparison.Ordinal);
			list[0].IndexOf(".com", 0, StringComparison.Ordinal);
			list[0].IndexOf(".com", 0, 5, StringComparison.Ordinal);
			list[0].IndexOf(list[1], 0, 10, StringComparison.Ordinal);
		}
		
		public void StringIndexOfStringCallsWithComparison(List<string> list)
		{
			list[0].IndexOf(".com", StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(".com", 0, StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(".com", 0, 5, StringComparison.OrdinalIgnoreCase);
			list[0].IndexOf(list[1], 0, 10, StringComparison.OrdinalIgnoreCase);
		}
		
		public void StringIndexOfCharCalls(List<string> list)
		{
			list[0].IndexOf('.');
			Environment.CommandLine.IndexOf('/');
		}
		
		public void OtherIndexOfCalls(List<string> list)
		{
			list.IndexOf(".com");
		}
	}
}

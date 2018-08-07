using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// The support class to analyze text.
	/// </summary>
	public static class TextAnalyzer
	{
		/// <summary>
		/// Splits <paramref name="text"/> with <paramref name="nest"/> and <paramref name="unnest"/> strings.
		/// And assigns depth to each splitted text.
		/// <para>Depth is assigned according to the following rules.</para>
		/// <para>- Default depth is 0.</para>
		/// <para>- Each time <paramref name="nest"/> string appears,
		/// adds 1 to the depth and assigns its depth to all strings after <paramref name = "nest" /></para>
		/// <para>- Each time <paramref name="unnest"/> string appears,
		/// subtracts 1 from the depth and assigns its depth to all strings after <paramref name = "unnest" /></para>
		/// </summary>
		/// <param name="text">Text to be splitted</param>
		/// <param name="nest">Separator(increasing depth)</param>
		/// <param name="unnest">Separator(decreasing depth)</param>
		/// <returns>List of Splitted text and depth</returns>
		/// <exception cref="ArgumentNullException">If at least one argument is null</exception>
		public static List<DepthText> SplitWithDepth(string text, string nest, string unnest)
		{
			if((text == null) || (nest == null) || (unnest == null))
				throw new ArgumentNullException();

			var words = text.Split(new[] { nest }, StringSplitOptions.None);
			var depthWords = new List<DepthText>(words.Length);
			var depthCount = 0;

			for(var i = 0; i < words.Length; i++)
			{
				var subWords = words[i].Split(new[] { unnest }, StringSplitOptions.None);

				foreach(var sub in subWords)
				{
					depthWords.Add(new DepthText(sub, depthCount));
					depthCount--;   // number of correct decrementation is subWord.Length-1, but decrements subWord.Length times
				}

				// Increased the depth by 1 for the next loop
				// And incremented one more because of wasteful subtraction in above loop
				// Since the above loop passes at least once, it does not become 0 decrementation and 2 incrementation
				depthCount += 2;
			}

			return depthWords;
		}

		/// <summary>
		/// Splits a text with <paramref name="separator"/>.
		/// If <paramref name="depthFilter"/> is specified, substrings which are enclosed by <paramref name="nest"/> and <paramref name="unnest"/>
		/// and whose depth is out of <paramref name="depthFilter"/> are not splitted.
		/// </summary>
		/// <param name="text">Text to be splitted</param>
		/// <param name="separator">Separator</param>
		/// <param name="nest">Text increasing depth</param>
		/// <param name="unnest">Text decreasing depth</param>
		/// <param name="depthFilter">Depth filter</param>
		/// <returns>Collection of splitted text</returns>
		/// <exception cref="ArgumentNullException">If at least one argument of <paramref name="text"/>, 
		/// <paramref name="separator"/>, <paramref name="nest"/>, <paramref name="unnest"/> is null</exception>
		public static List<string> Split(this string text, string separator, string nest, string unnest, Func<int, bool> depthFilter = null)
		{
			if((text == null) || (separator == null) || (nest == null) || (unnest == null))
				throw new ArgumentNullException();

			var depthTexts = SplitWithDepth(text, nest, unnest);
			var words = new List<string>();
			var subDepthTexts = new List<DepthText>(depthTexts.Count);
			
			for(var i = 0; i < depthTexts.Count; i++)
			{
				var dText = depthTexts[i];

				if(!(depthFilter?.Invoke(dText.Depth) ?? true))
				{
					subDepthTexts.Add(dText);
					continue;
				}
				
				var splitted = dText.Text.Split(new[] { separator }, StringSplitOptions.None);

				if(splitted.Length <= 1)
				{
					subDepthTexts.Add(dText);
					continue;
				}

				subDepthTexts.Add(new DepthText(splitted[0], dText.Depth));
				words.Add(subDepthTexts.Marge(nest, unnest));

				foreach(var s in splitted.Skip(1).Take(splitted.Length - 2))
				{
					words.Add(s);
				}

				subDepthTexts.Clear();
				subDepthTexts.Add(new DepthText(splitted.Last(), dText.Depth));
			}

			if(subDepthTexts.Count > 0)
			{
				words.Add(subDepthTexts.Marge(nest, unnest));
			}

			return words;
		}

		/// <summary>
		/// Splits each text in <paramref name="texts"/> with <paramref name="separator"/>.
		/// Each splitted text is assigned to original text's depth.
		/// </summary>
		/// <param name="texts">Collection of <see cref="DepthText"/></param>
		/// <param name="separator">Separator</param>
		/// <returns>List of splitted <see cref="DepthText"/></returns>
		public static List<DepthText> Split(this IEnumerable<DepthText> texts, string separator)
		{
			if(texts == null)
				throw new ArgumentNullException();

			var list = new List<DepthText>();

			foreach(var text in texts)
			{
				list.AddRange(text.Split(separator));
			}

			return list;
		}

		/// <summary>
		/// Marges <see cref="DepthText"/>[s] to generate a string.
		/// Insert <paramref name="nest"/> If marge text and more deep text,
		/// and insert <paramref name="unnest"/> If marge text and less deep text.
		/// </summary>
		/// <param name="texts">Collection of <see cref="DepthText"/></param>
		/// <param name="nest">Separator(increasing depth)</param>
		/// <param name="unnest">Separator(decreasing depth)</param>
		/// <returns>A string by marging <see cref="DepthText"/>[s]</returns>
		public static string Marge(this IEnumerable<DepthText> texts, string nest, string unnest)
		{
			if(texts == null)
				throw new ArgumentNullException();

			var prevWord = texts.FirstOrDefault();
			var builder = new StringBuilder()
				.Append(prevWord?.Text ?? string.Empty);

			foreach(var word in texts.Skip(1))
			{
				if(prevWord.Depth < word.Depth)
				{
					builder.Append(nest);
				}
				else if(prevWord.Depth > word.Depth)
				{
					builder.Append(unnest);
				}

				builder.Append(word.Text);
				prevWord = word;
			}

			return builder.ToString();
		}

		/// <summary>
		/// Returns a string by concatenating n <paramref name="str"/>.
		/// </summary>
		/// <param name="str">Base string</param>
		/// <param name="n">Number of concatenation</param>
		/// <returns>A concatinated string</returns>
		private static string ContinuousString(string str, int n)
		{
			return string.Join(string.Empty, Enumerable.Range(0, n).Select(_ => str));
		}
	}
}

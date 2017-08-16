using System;
using System.Collections.Generic;
using System.Text;

using con = System.Console;

namespace cscon
{
	class Program
	{
		const int maxXCount = 2000;
		private static List<Coset> X;
		private static int[][] G, H;
		private static Coset activeNode;

		static void Main(string[] args)
		{
			string[] Gwords = { "aaBABA","bbBABA" };
			string[] Hgens = {};

			TranslateGroupWords(Gwords, Hgens);
			X = new List<Coset> { new Coset() };
			activeNode = X[0];

			List<int[]> tmp = new List<int[]>(H);
			tmp.AddRange(G);
			CompleteNode(tmp.ToArray());

			con.WriteLine("Entering main procedure");
			while (activeNode != null && X.Count < maxXCount)
			{
				CompleteNode(G);
				int pos = X.IndexOf(activeNode);
				activeNode = (pos == X.Count - 1) ? null : X[pos + 1];
			}

			pauser("Enumeration complete");
			con.ForegroundColor = ConsoleColor.Green;
			for (int i = 0; i < X.Count; i++)
				con.WriteLine("X" + X[i]);
			con.ForegroundColor = ConsoleColor.White;
			con.WriteLine("Total number of cosets: " + X.Count);
			con.ReadKey();
		}

		private static void TranslateGroupWords(string[] Gwords, string[] Hgens)
		{
			Dictionary<char, int> opTable = new Dictionary<char, int>();

			G = new int[Gwords.Length][];
			int counter = 0;
			for (int i = 0; i < Gwords.Length; i++)
			{
				var temp = Gwords[i].ToLower().ToCharArray();
				for (int j = 0; j < temp.Length; j++)
				{
					if (!opTable.ContainsKey(temp[j]))
						opTable.Add(temp[j], counter++);
				}
			}
			Coset.NumOp = opTable.Count;

			for (int i = 0; i < G.Length; i++)
			{
				G[i] = new int[Gwords[i].Length];
				var temp = Gwords[i].ToCharArray();
				var templower = Gwords[i].ToLower().ToCharArray();
				for (int j = 0; j < temp.Length; j++)
				{
					char letter = temp[j];
					G[i][j] = opTable.ContainsKey(letter) ? opTable[letter] : (opTable[templower[j]] + Coset.NumOp);
				}
			}

			H = new int[Hgens.Length][];
			for (int i = 0; i < H.Length; i++)
			{
				H[i] = new int[Hgens[i].Length];
				var temp = Hgens[i].ToCharArray();
				var templower = Hgens[i].ToLower().ToCharArray();
				for (int j = 0; j < temp.Length; j++)
				{
					char letter = temp[j];
					H[i][j] = opTable.ContainsKey(letter) ? opTable[letter] : (opTable[templower[j]] + Coset.NumOp);
				}
			}
		}

		private static void CompleteNode(int[][] Gx)
		{
			int workingIdx = activeNode.Idx;
			con.WriteLine("Analyzing X" + workingIdx);
			spaceF++;
			//TODO: implement "Lookahead" checking procedure
			for (int i = 0; i < Gx.Length; i++)
			{
				MakeDefinition(Gx[i]);
				if (activeNode.Idx == -1)
				{
					activeNode = X[0];
					pauser("Interrupt X" + workingIdx + " because removal");
					spaceF--;
					return;
				}
			}
			pauser("Complete X" + workingIdx);
			spaceF--;
		}

		private static void MakeDefinition(int[] rel)
		{
			Coset prev = activeNode;
			con.WriteLine(" Checking path " + printOp(rel) + " from X" + activeNode.Idx);
			spaceF++;

			for (int i = 0; i < rel.Length; i++)
			{
				int curOp = rel[i];
				Coset newNode;
				if (curOp < Coset.NumOp)    //forward op
				{
					if (prev.Relations[curOp].To == null) //no existing path
					{
						newNode = new Coset();
						X.Add(newNode);
						newNode.Relations[curOp].Back = prev;
						prev.Relations[curOp].To = newNode;
						pauser("X" + prev.Idx + " with Op" + curOp + " generated X" + newNode.Idx);
						prev = newNode;
						continue;
					}
					// existing path
					prev = prev.Relations[curOp].To;
					continue;
				}
				//inverse op
				curOp -= Coset.NumOp;
				if (prev.Relations[curOp].Back == null) //no existing path
				{
					newNode = new Coset();
					X.Add(newNode);
					newNode.Relations[curOp].To = prev;
					prev.Relations[curOp].Back = newNode;
					pauser("X" + prev.Idx + " with Op" + (curOp + Coset.NumOp) + " generated X" + newNode.Idx);
					prev = newNode;
					continue;
				}
				// existing path
				prev = prev.Relations[curOp].Back;
			}

			MergeNode(prev, activeNode);
			spaceF--;
		}

		private static void MergeNode(Coset source, Coset target)
		{
			if (source == target || IsInvalid(source) || IsInvalid(target))
				return;
			spaceF++;
			pauser("Conflict: merging X" + source.Idx + " into X" + target.Idx);
			Coset[] sTo = new Coset[Coset.NumOp];
			Coset[] sBack = new Coset[Coset.NumOp];
			Coset[] tTo = new Coset[Coset.NumOp];
			Coset[] tBack = new Coset[Coset.NumOp];
			X.Remove(source);
			pauser("Removed X" + source);
			source.Invalidate();
			for (int i = 0; i < Coset.NumOp; i++)
			{
				//cut off connection and form template
				sTo[i] = source.Relations[i].To;
				sBack[i] = source.Relations[i].Back;
				tTo[i] = target.Relations[i].To;
				tBack[i] = target.Relations[i].Back;

//				source.Relations[i].To = null;
//				source.Relations[i].Back = null;
				if (sTo[i] != null)
				{
					sTo[i].Relations[i].Back = null;
				}
				if (sBack[i] != null)
				{
					sBack[i].Relations[i].To = null;
				}
			}
			for (int i = 0; i < Coset.NumOp; i++)
			{
				if (IsInvalid(tTo[i])&& sTo[i] != null)
				{
					target.Relations[i].To = sTo[i];
//					if (sTo[i] != null)
						sTo[i].Relations[i].Back = target;
				}
				if (tTo[i]==source)
					target.Relations[i].To = target;

				if (IsInvalid(tBack[i])&& sBack[i] != null)
				{
					target.Relations[i].Back = sBack[i];
//					if (sBack[i] != null)
						sBack[i].Relations[i].To = target;
				}
				if (tBack[i]==source)
					target.Relations[i].Back = target;
			}
			for (int i = 0; i < Coset.NumOp; i++)
			{
				MergeNode(sTo[i], target.Relations[i].To);
				MergeNode(sBack[i], target.Relations[i].Back);
//				MergeNode(sTo[i], tTo[i]);
//				MergeNode(sBack[i], tBack[i]);
			}
			spaceF--;
		}

		static bool IsInvalid(Coset t)
		{
			return t == null || t.Idx == -1;
		}

		class Coset
		{
			public static int NumOp;
			private static int IdCounter;
			public readonly Transform[] Relations;
			public int Idx;

			public Coset()
			{
				Idx = IdCounter++;
				Relations = new Transform[NumOp];
				for (int i = 0; i < NumOp; i++)
					Relations[i] = new Transform();
			}

			public void Invalidate()
			{
				Idx = -1;
			}

			public override string ToString()
			{
				StringBuilder str = new StringBuilder($"{Idx}: ");
				for (int i = 0; i < NumOp; i++)
					str.AppendFormat($"Op{i}({Relations[i]}) ");
				return str.ToString();
			}
		}

		struct Transform
		{
			public Coset To;
			public Coset Back;
			public override string ToString()
			{
				return $"{Back?.Idx.ToString() ?? "--"},{To?.Idx.ToString() ?? "--"}";
			}
		}

		private static int spaceF = 0;
		static void pauser(string info)
		{
			con.WriteLine("{0," + spaceF + "}{1}", "", info);
			return;
			while (true)
			{
				var k = con.ReadKey(true);
				var c = con.ForegroundColor;
				switch (k.KeyChar)
				{
					case 'q':
						Environment.Exit(0);
						break;
					case 'x':
						con.ForegroundColor = ConsoleColor.Cyan;
						for (int i = 0; i < X.Count; i++)
							con.WriteLine("X" + X[i]);
						break;
					default:
						return;
				}
				con.ForegroundColor = c;
			}
		}

		static string printOp(int[] op)
		{
			StringBuilder st = new StringBuilder();
			for (int i = 0; i < op.Length; i++)
				st.Append(op[i]);
			return st.ToString();
		}
	}

}

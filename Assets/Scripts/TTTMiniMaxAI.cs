using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TTTMiniMaxAI
{
	[Serializable]
	private class Node
	{
		public Symbol[] field;
		public Node father;
		public List<Node> children;
		public int value;
		public int level;
		public bool isMax;
		public int move;
		public int victories;
		public int draws;
		public int loses;

		public Node(Symbol[] field)
		{
			this.field = GetFieldDeepCopy(field);
			father = null;
			children = new List<Node>();
			value = int.MinValue;
			level = 0;
			isMax = true;
			move = int.MinValue;
		}

		public Node(Symbol[] field, Symbol symbol, int positionI, int positionJ, Node father, int initialValue)
		{
			this.father = father;
			Symbol[] newField = GetFieldDeepCopy(field);
			newField[positionI * 3 + positionJ] = symbol;
			this.field = newField;
			children = new List<Node>();
			father.children.Add(this);
			value = initialValue;
			level = father.level + 1;
			isMax = symbol == Symbol.O;
			move = positionI * 3 + positionJ;
		}

		/// <summary>
		/// Returns a deep copy of the field (a new object with a new reference, not a shallow copy)
		/// </summary>
		/// <param name="toFieldCopy">Field to copy</param>
		/// <returns>A deep copy of the given field</returns>
		private Symbol[] GetFieldDeepCopy(Symbol[] toFieldCopy)
		{
			Symbol[] newField = new Symbol[9];
			for (int i = 0; i < 9; i++)
				newField[i] = toFieldCopy[i];
			return newField;
		}
	}

	/// <summary>
	/// Calculates the next move of the CPU using Minimax AI
	/// </summary>
	/// <param name="initialSerialized">Initial serialized version of the field (e.g. "1_0_1_0_1" with 0->O, 1->X, _->Empty)</param>
	/// <param name="maxTreeDeepness">Maximum deepness of the search tree, from 1 to 9. 9 Generates the complete tree</param>
	/// <returns>The index in which the AI wants to place the next symbol</returns>
	public int GetNextMove(string initialSerialized, int maxTreeDeepness)
	{
		Symbol[] initial = DeserializeField(initialSerialized);
		Node initialNode = new Node(initial);


		AlphaBeta(initialNode, maxTreeDeepness, int.MinValue, int.MaxValue, true);
		/*

		Queue<Node> queue = new Queue<Node>();
		queue.Enqueue(initialNode);


		// Generate tree and calculate node values
		while (queue.Count != 0)
		{
			Node actual = queue.Dequeue();
			Symbol nextSymbol = actual.isMax ? Symbol.X : Symbol.O;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (actual.field[i * 3 + j] == Symbol.Empty)
					{
						Node temp = new Node(actual.field, nextSymbol, i, j, actual);

						if (temp.field != null && temp.level < maxTreeDeepness && CheckVictory(LinearTo2DArray(temp.field)) == Symbol.Empty)
							queue.Enqueue(temp);
					}
				}
			}
		}

		// Calculate minimax values
		CalculateLeavesValues(initialNode);
		initialNode.value = GetNodeValue(initialNode);


		// Search the number of victories, draws and loses possible from this node
		initialNode.victories = GetNodeVictories(initialNode);
		initialNode.draws = GetNodeDraws(initialNode);
		initialNode.loses = GetNodeLoses(initialNode);

		*/

		File.WriteAllText("tree.json", JsonUtility.ToJson(initialNode));

		// Search the best move
		int max = int.MinValue;
		int selectedMove = 0;

		foreach (Node child in initialNode.children)
			if (child.value > max)
			{
				selectedMove = child.move;
				max = child.value;
			}


		/*
		List<Node> equivalentMoves = new List<Node>();
		foreach (Node child in initialNode.children)
			if (child.value.Equals(initialNode.value))
				equivalentMoves.Add(child);

		int selectedMove = GetBestMoveBetweenEquivalentOnes(equivalentMoves);
		*/

		// Clean the tree to avoid memory leaks
		CleanRecursive(initialNode);

		return selectedMove;
	}

	/// <summary>
	/// Calculates the leaves nodes values starting from the given node as root
	/// </summary>
	/// <param name="node">The root of the current subtree</param>
	private void CalculateLeavesValues(Node node)
	{
		if (node.children.Count == 0)
			CalculateNodeValue(node);
		else
			foreach (Node child in node.children)
				CalculateLeavesValues(child);
	}

	/// <summary>
	/// Calculates the node value. It sets a low value if min won, a high value if max won,
	/// 0 if it is a draw or an evaluated value if it is a terminal node but the game isn't ended
	/// </summary>
	/// <param name="node">The node to calculate the value on</param>
	private void CalculateNodeValue(Node node)
	{
		switch (CheckVictory(LinearTo2DArray(node.field)))
		{
			case Symbol.X:
				node.value = 15 - node.level;
				break;
			case Symbol.O:
				node.value = node.level - 15;
				break;
			case Symbol.Empty:
				node.value = IsFieldFullyFilled(node.field) ? 0 : EvaluateField(LinearTo2DArray(node.field));
				break;
		}
	}

	/// <summary>
	/// Evaluates a non-terminal field based on the following heuristic:
	/// Number of possible winning rows, cols and diagonals for max player - number of possible winning rows, cols and diagonals for min player
	/// </summary>
	/// <param name="field">The field to evaluate</param>
	/// <returns>The value of the given field based on the heuristic</returns>
	private int EvaluateField(Symbol[,] field)
	{
		int maxPossibleWinnings = 0;
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnRow(field, Symbol.X, i) == 0)
				maxPossibleWinnings++;
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnCol(field, Symbol.X, i) == 0)
				maxPossibleWinnings++;
		if (CountSymbolsOnMainDiagonal(field, Symbol.X) == 0)
			maxPossibleWinnings++;
		if (CountSymbolsOnAntidiagonal(field, Symbol.X) == 0)
			maxPossibleWinnings++;

		int minPossibleWinnings = 0;
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnRow(field, Symbol.O, i) == 0)
				minPossibleWinnings++;
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnCol(field, Symbol.O, i) == 0)
				minPossibleWinnings++;
		if (CountSymbolsOnMainDiagonal(field, Symbol.O) == 0)
			minPossibleWinnings++;
		if (CountSymbolsOnAntidiagonal(field, Symbol.O) == 0)
			minPossibleWinnings++;

		return maxPossibleWinnings - minPossibleWinnings;
	}

	/// <summary>
	/// Calculates minimax values for the tree with the given node as root
	/// </summary>
	/// <param name="node">The root of the current subtree</param>
	/// <returns>The minimax value of the given node</returns>
	private int GetNodeValue(Node node)
	{
		if (node.children.Count == 0)
			return node.value;
		if (node.isMax)
		{
			int max = int.MinValue;
			foreach (Node child in node.children)
			{
				max = Math.Max(max, GetNodeValue(child));
			}
			node.value = max;
			return node.value;
		}
		else
		{
			int min = int.MaxValue;
			foreach (Node child in node.children)
			{
				min = Math.Min(min, GetNodeValue(child));
			}
			node.value = min;
			return node.value;
		}
	}

	/// <summary>
	/// Calculates the number of victories for every node of the subtree having the given node as root
	/// </summary>
	/// <param name="node">The root of the current subtree</param>
	/// <returns>The number of victory from the given node</returns>
	private int GetNodeVictories(Node node)
	{
		if (node.children.Count == 0)
			return CheckVictory(LinearTo2DArray(node.field)) == Symbol.X ? 1 : 0;

		int sum = 0;
		foreach (Node child in node.children)
			sum += GetNodeVictories(child);

		node.victories = sum;
		return node.victories;
	}

	/// <summary>
	/// Calculates the number of draws for every node of the subtree having the given node as root
	/// </summary>
	/// <param name="node">The root of the current subtree</param>
	/// <returns>The number of draws from the given node</returns>
	private int GetNodeDraws(Node node)
	{
		if (node.children.Count == 0)
			return CheckVictory(LinearTo2DArray(node.field)) == Symbol.Empty && IsFieldFullyFilled(node.field) ? 1 : 0;

		int sum = 0;
		foreach (Node child in node.children)
			sum += GetNodeDraws(child);

		node.draws = sum;
		return node.draws;
	}

	/// <summary>
	/// Calculates the number of loses for every node of the subtree having the given node as root
	/// </summary>
	/// <param name="node">The root of the current subtree</param>
	/// <returns>The number of loses from the given node</returns>
	private int GetNodeLoses(Node node)
	{
		if (node.children.Count == 0)
			return CheckVictory(LinearTo2DArray(node.field)) == Symbol.O ? 1 : 0;

		int sum = 0;
		foreach (Node child in node.children)
			sum += GetNodeLoses(child);

		node.loses = sum;
		return node.loses;
	}

	/// <summary>
	/// Selects the best move between the equivalent best ones based on the victory ratio ( W/(W+D+L) )
	/// </summary>
	/// <param name="nodes">The list of the equivalent best moves from minimax algorithm</param>
	/// <returns>The move with the highest victory ratio</returns>
	private int GetBestMoveBetweenEquivalentOnes(List<Node> nodes)
	{
		int selectedMove = nodes[0].move;
		float maxRatio = (float)nodes[0].victories / (nodes[0].victories + nodes[0].draws + nodes[0].loses);
		for (int i = 1; i < nodes.Count; i++)
		{
			if ((float)nodes[i].victories / (nodes[i].victories + nodes[i].draws + nodes[i].loses) > maxRatio)
			{
				maxRatio = (float)nodes[i].victories / (nodes[i].victories + nodes[i].draws + nodes[i].loses);
				selectedMove = nodes[i].move;
			}
		}
		return selectedMove;
	}

	/// <summary>
	/// Returns a linear array representation of the field
	/// </summary>
	/// <param name="serializedField">Serialized version of the field (e.g. "1_0_1_0_1" with 0->O, 1->X, _->Empty)</param>
	/// <returns>A linear array representation of the field</returns>
	private Symbol[] DeserializeField(string serializedField)
	{
		Symbol[] linearField = new Symbol[9];
		for (int i = 0; i < serializedField.Length; i++)
		{
			switch (serializedField[i])
			{
				case '0':
					linearField[i] = Symbol.O;
					break;
				case '1':
					linearField[i] = Symbol.X;
					break;
				case '_':
					linearField[i] = Symbol.Empty;
					break;
			}
		}

		return linearField;
	}

	/// <summary>
	/// Converts a linear array field into a 2-dimensional array
	/// </summary>
	/// <param name="linearField">The linear array representation of the field</param>
	/// <returns>The 2-dimensional array representation of the field</returns>
	private Symbol[,] LinearTo2DArray(Symbol[] linearField)
	{
		Symbol[,] field = new Symbol[3, 3];
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				field[i, j] = linearField[i * 3 + j];
		return field;
	}

	/// <summary>
	/// Checks if the field is fully filled or not (no remaining empty spaces)
	/// </summary>
	/// <param name="field">The field to check</param>
	/// <returns>True if the field is fully filled, false otherwise</returns>
	private bool IsFieldFullyFilled(Symbol[] field)
	{
		return !field.Contains<Symbol>(Symbol.Empty);
	}

	/// <summary>
	/// Checks if and who won the game in the given field
	/// </summary>
	/// <param name="field">The field to evaluate</param>
	/// <returns>The symbol of the winner or Symbol.Empty if there is no winner</returns>
	private Symbol CheckVictory(Symbol[,] field)
	{
		if (CheckVictoryForSymbol(field, Symbol.O))
			return Symbol.O;
		if (CheckVictoryForSymbol(field, Symbol.X))
			return Symbol.X;
		return Symbol.Empty;
	}

	/// <summary>
	/// Checks if the given symbol won the game in the given field
	/// </summary>
	/// <param name="field">The field to evaluate</param>
	/// <param name="symbol">The symbol to check if it won</param>
	/// <returns>True if the given symbol won, false otherwise</returns>
	private bool CheckVictoryForSymbol(Symbol[,] field, Symbol symbol)
	{
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnRow(field, symbol, i) == 3)
				return true;
		for (int i = 0; i < 3; i++)
			if (CountSymbolsOnCol(field, symbol, i) == 3)
				return true;
		if (CountSymbolsOnMainDiagonal(field, symbol) == 3)
			return true;
		if (CountSymbolsOnAntidiagonal(field, symbol) == 3)
			return true;
		return false;
	}

	/// <summary>
	/// Counts the given symbol occurrences on the given row of the given field
	/// </summary>
	/// <param name="field">The field to search on</param>
	/// <param name="symbol">The symbol to search</param>
	/// <param name="row">The row of the field to iterate on</param>
	/// <returns>The number of the given symbol occurrences on the given row of the given field</returns>
	private int CountSymbolsOnRow(Symbol[,] field, Symbol symbol, int row)
	{
		int count = 0;
		for (int i = 0; i < 3; i++)
			if (field[row, i] == symbol)
				count++;
		return count;
	}

	/// <summary>
	/// Counts the given symbol occurrences on the given column of the given field
	/// </summary>
	/// <param name="field">The field to search on</param>
	/// <param name="symbol">The symbol to search</param>
	/// <param name="col">The column of the field to iterate on</param>
	/// <returns>The number of the given symbol occurrences on the given column of the given field</returns>
	private int CountSymbolsOnCol(Symbol[,] field, Symbol symbol, int col)
	{
		int count = 0;
		for (int i = 0; i < 3; i++)
			if (field[i, col] == symbol)
				count++;
		return count;
	}

	/// <summary>
	/// Counts the given symbol occurrences on the main diagonal of the given field
	/// </summary>
	/// <param name="field">The field to search on</param>
	/// <param name="symbol">The symbol to search</param>
	/// <returns>The number of the given symbol occurrences on the main diagonal of the given field</returns>
	private int CountSymbolsOnMainDiagonal(Symbol[,] field, Symbol symbol)
	{
		int count = 0;
		for (int i = 0; i < 3; i++)
			if (field[i, i] == symbol)
				count++;
		return count;
	}

	/// <summary>
	/// Counts the given symbol occurrences on the antidiagonal of the given field
	/// </summary>
	/// <param name="field">The field to search on</param>
	/// <param name="symbol">The symbol to search</param>
	/// <returns>The number of the given symbol occurrences on the antidiagonal of the given field</returns>
	private int CountSymbolsOnAntidiagonal(Symbol[,] field, Symbol symbol)
	{
		int count = 0;
		for (int i = 0; i < 3; i++)
			if (field[i, 2 - i] == symbol)
				count++;
		return count;
	}

	/// <summary>
	/// Cleans the tree recursively starting from the given root
	/// </summary>
	/// <param name="root">The node on which to start the cleaning</param>
	private void CleanRecursive(Node root)
	{
		if (root.children.Count == 0)
			root.father = null;
		else
		{
			foreach (Node child in root.children)
				CleanRecursive(child);
			root.children.Clear();
		}
	}

	private int AlphaBeta(Node initialNode, int depth, int alpha, int beta, bool isMax)
	{
		if (depth == 0 || CheckVictory(LinearTo2DArray(initialNode.field)) != Symbol.Empty)
		{
			CalculateNodeValue(initialNode);
			return initialNode.value;
		}

		Symbol nextSymbol = isMax ? Symbol.X : Symbol.O;

		if (isMax)
		{
			int value = int.MinValue;

			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					if (initialNode.field[i * 3 + j] == Symbol.Empty)
						new Node(initialNode.field, nextSymbol, i, j, initialNode, int.MaxValue);

			foreach(Node child in initialNode.children)
			{
				value = Mathf.Max(value, AlphaBeta(child, depth - 1, alpha, beta, false));
				if (value > beta)
					break;
				alpha = Mathf.Max(alpha, value);
			}
			initialNode.value = value;
			return value;
		}
		else
		{
			int value = int.MaxValue;

			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					if (initialNode.field[i * 3 + j] == Symbol.Empty)
						new Node(initialNode.field, nextSymbol, i, j, initialNode, int.MinValue);

			foreach (Node child in initialNode.children)
			{
				value = Mathf.Min(value, AlphaBeta(child, depth - 1, alpha, beta, true));
				if (value < alpha)
					break;
				beta = Mathf.Min(beta, value);
			}
			initialNode.value = value;
			return value;
		}
	}

}
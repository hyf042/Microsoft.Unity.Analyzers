/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

namespace Microsoft.Unity.Analyzers.StyleCop;

internal class IndentationSettings
{
	/// <summary>
	/// This is the backing field for the <see cref="IndentationSize"/> property.
	/// </summary>
	private readonly int indentationSize;

	/// <summary>
	/// This is the backing field for the <see cref="TabSize"/> property.
	/// </summary>
	private readonly int tabSize;

	/// <summary>
	/// This is the backing field for the <see cref="UseTabs"/> property.
	/// </summary>
	private readonly bool useTabs;

	/// <summary>
	/// Initializes a new instance of the <see cref="IndentationSettings"/> class.
	/// </summary>
	protected internal IndentationSettings()
	{
		this.indentationSize = 4;
		this.tabSize = 4;
		this.useTabs = false;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IndentationSettings"/> class.
	/// </summary>
	protected internal IndentationSettings(int indentationSize, int tabSize, bool useTabs)
	{
		this.indentationSize = IndentationSize;
		this.tabSize = tabSize;
		this.useTabs = useTabs;
	}

	public int IndentationSize =>
		this.indentationSize;

	public int TabSize =>
		this.tabSize;

	public bool UseTabs =>
		this.useTabs;
}

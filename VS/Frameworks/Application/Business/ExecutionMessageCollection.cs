using System;
using System.Collections;
using System.Text;

namespace App.Framework.Business
{
	/// <summary>
	/// Represents a collection of <see cref="ExecutionMessage"/> objects.
	/// </summary>
	// Greg Wood - Friday, June 18, 2000

	#region ExecutionMessageCollection

	public delegate void ExecutionMessageEventHandler(ExecutionMessage message);
	
	/// <summary>
	/// A type safe collection of type <see cref="ExecutionMessage"/> objects.
	/// </summary>
	/// <remarks>
	/// Used to manage a collection of ExecutionMessage objects.
	/// <seealso cref="WarningMessage"/>
	/// <seealso cref="ErrorMessage"/>
	/// <seealso cref="ExecutionMessage"/>
	/// </remarks>
	// Greg Wood - Friday, June 18, 2000
	[Serializable]
	public class ExecutionMessageCollection : CollectionBase
	{
		/// <summary>
		/// Raised when an item is added to the list.
		/// </summary>
		/// <remarks>
		/// This would include a call to <see cref="Add"/>,
		/// <see cref="AddRange(App.Framework.Business.ExecutionMessage[])"/> and <see cref="AddRange(App.Framework.Business.ExecutionMessageCollection)"/> or <see cref="Insert"/> methods.
		/// <seealso cref="Add"/>
		/// <seealso cref="AddRange(App.Framework.Business.ExecutionMessage[])"/> and <seealso cref="AddRange(App.Framework.Business.ExecutionMessageCollection)"/>
		/// <seealso cref="Insert"/>
		/// </remarks>
		public event ExecutionMessageEventHandler ExecutionMessageAdd;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <example>
		/// <code>ExecutionMessageCollection ToReturn = new ExecutionMessageCollection();</code>
		/// </example>
		public ExecutionMessageCollection() : base()
		{
		}

		/// <summary>
		/// Construct class and add a list of messages.
		/// </summary>
		/// <param name="messages">Array of ExecutionMessages.</param>
		public ExecutionMessageCollection(ExecutionMessage[] messages)
		{
			this.AddRange(messages);
		}

		/// <summary>
		/// Construct class and add a list of messages.
		/// </summary>
		/// <param name="messages">ExecutionMessageCollection to add to this list.</param>
		public ExecutionMessageCollection(ExecutionMessageCollection messages)
		{
			this.AddRange(messages);
		}

		/// <summary>
		/// Adds an <see cref="ExecutionMessage"/> to the end of the collection.
		/// </summary>
		/// <example>
		/// <code>
		/// ExecutionMessageCollection ToReturn = new ExecutionMessageCollection();
		/// ToReturn.Add(new WarningMessage("Item not found."));
		/// </code>
		/// </example>
		/// <param name="newMessage">The <see cref="ExecutionMessage"/> to be added to the end of the collection.</param>
		public void Add(ExecutionMessage newMessage)
		{
			this.OnExecutionMessageAdd(newMessage);
			this.List.Add(newMessage);
		}

		/// <summary>
		/// Adds an array of <see cref="ExecutionMessage">ExecutionMessages</see> to the end of the collection.
		/// </summary>
		/// <param name="newMessages">Array of <see cref="ExecutionMessage">ExecutionMessages</see>.</param>
		public void AddRange(ExecutionMessage[] newMessages)
		{
			foreach ( ExecutionMessage em in newMessages )
                this.Add(em);
		}

		/// <summary>
		/// Adds a collection of <see cref="ExecutionMessage">ExecutionMessages</see> to the end of the collection.
		/// </summary>
		/// <param name="newMessages">Collection of <see cref="ExecutionMessage">ExecutionMessages</see>.</param>
		public void AddRange(ExecutionMessageCollection newMessages)
		{
			foreach ( ExecutionMessage em in newMessages )
				this.Add(em);
		}

		/// <summary>
		/// Determines whether the collection contains a specific <see cref="ExecutionMessage"/>.
		/// </summary>
		/// <param name="seekMessage">The <see cref="ExecutionMessage"/> to locate in the collection.</param>
		/// <returns>true if the collection contains the specified ExecutionMessage; otherwise, false.</returns>
		public bool Contains(ExecutionMessage seekMessage)
		{
			return this.List.Contains(seekMessage);
		}

		/// <summary>
		/// Searches for the specified <see cref="ExecutionMessage"/> and returns the zero-based 
		/// index of the first occurrence within the entire collection.
		/// </summary>
		/// <param name="seekMessage">The <see cref="ExecutionMessage"/> to locate in the collection.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of value within the entire collection, if found; otherwise, -1.
		/// </returns>
		public int IndexOf(ExecutionMessage seekMessage)
		{
			return this.List.IndexOf(seekMessage);
		}

		/// <summary>
		/// Inserts an <see cref="ExecutionMessage"/> into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="newMessage">The <see cref="ExecutionMessage"/> to insert.</param>
		public void Insert(int index, ExecutionMessage newMessage)
		{
			this.OnExecutionMessageAdd(newMessage);
			this.List.Insert(index, newMessage);
		}

		/// <summary>
		/// Removes the first occurrence of a specific <see cref="ExecutionMessage"/> from the collection.
		/// </summary>
		/// <param name="messageToRemove">The <see cref="ExecutionMessage"/> to remove.</param>
		public void Remove(ExecutionMessage messageToRemove)
		{
			this.List.Remove(messageToRemove);
		}

		/// <summary>
		/// An array representation of the collection.
		/// </summary>
		/// <returns>An array of the current state of the collection.</returns>
		public ExecutionMessage[] ToArray()
		{
			return (ExecutionMessage[]) this.InnerList.ToArray(typeof(ExecutionMessage));
		}

		/// <summary>
		/// Invoked when the <see cref="ExecutionMessageAdd"/> event is raised.
		/// </summary>
		/// <param name="message">The added <see cref="ExecutionMessage"/>.</param>
		protected virtual void OnExecutionMessageAdd(ExecutionMessage message)
		{
			if (this.ExecutionMessageAdd != null)
				this.ExecutionMessageAdd(message);
		}

		/// <summary>
		/// True if this collection contains at least one <see cref="ErrorMessage"/>.
		/// </summary>
		public bool HasError()
		{
			foreach (ExecutionMessage Msg in this.InnerList)
			{
				if ( Msg is ErrorMessage )
					return true;
			}

			return false;
		}

		/// <summary>
		/// True if this collection contains at least one <see cref="WarningMessage"/>.
		/// </summary>
		public bool HasWarning()
		{
			foreach (ExecutionMessage Msg in this.InnerList)
			{
				if ( Msg is WarningMessage )
					return true;
			}

			return false;
		}

		/// <summary>
		/// True if this collection contains at least one <see cref="UserMessage"/>.
		/// </summary>
		public bool HasUserMessage()
		{
			foreach (ExecutionMessage Msg in this.InnerList)
			{
				if ( Msg is UserMessage )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns a string containing the text of every ExecutionMessage in the collection.
		/// </summary>
		/// <remarks>
		/// The string returned will have each ExecutionMessage text separated by
		/// a newline character (System.Environment.NewLine).
		/// </remarks>
		/// <returns>String of text for every ExecutionMessage in the collection.</returns>
		public virtual string AsString()
		{
			StringBuilder ToReturn = new StringBuilder();

			foreach ( ExecutionMessage Msg in this.InnerList )
			{
				ToReturn.Append(Msg.Text);
				ToReturn.Append(System.Environment.NewLine);
			}
			return ToReturn.ToString();
		}
	}
	#endregion

    #region ExecutionMessageCollectionEventArgs
    public class ExecutionMessageCollectionEventArgs : EventArgs
    {
        private readonly ExecutionMessageCollection _messages;

        public ExecutionMessageCollectionEventArgs(ExecutionMessageCollection messages)
        {
            _messages = messages;
        }

        public ExecutionMessageCollection Messages
        {
            get { return _messages; }
        }
    }
    #endregion
}

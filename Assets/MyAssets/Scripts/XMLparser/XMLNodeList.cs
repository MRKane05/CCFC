using System.Collections;

public class XMLNodeList: ArrayList 
{
	public XMLNode Pop()
	{
		XMLNode item = null;
	
		if (this.Count>0)
		{
			item = (XMLNode)this[this.Count - 1];
			this.Remove(item);
		}
		
		return item;
	}
	
	public int Push(XMLNode item)
	{
		Add(item);
		
		return this.Count;
	}
}
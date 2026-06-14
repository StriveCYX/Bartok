using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;

[System.Serializable]
public class SlotDef
{
    public float        x;
    public float        y;
    public bool         faceUp = false;
    public string       layerName = "Default";
    public int          layerID = 0;
    public int          id;
    public List<int>    hiddenBy = new List<int>(); //	Unused	in	Bartok
    public float        rot;                        //	rotation	of	hands
    public string       type = "slot";
    public Vector2      stagger;
    public int          player;                     //	player	number	of	a	hand
    public Vector3      pos;                        //	pos	derived	from	x,	y,	&	multiplier
}
public class BartokLayout : MonoBehaviour
{
    [Header("Set	Dynamically")]
    public XDocument xmlr;
    public XElement root;

    public Vector2 multiplier;  //	Sets	the	spacing	of	the	tableau
                                //	SlotDef	references
    public List<SlotDef> slotDefs;  //	The	SlotDefs	hands
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //	Bartok	calls	this	method	to	read	in	the	BartokLayoutXML.xml	file
    public void ReadLayout(string xmlText)
    {
        xmlr = XDocument.Parse(xmlText);
        root = xmlr.Element("xml");

        multiplier.x = float.Parse(root.Element("multiplier").Attribute("x").Value);
        multiplier.y = float.Parse(root.Element("multiplier").Attribute("y").Value);
        //	Read	in	the	slots
        SlotDef tSD;
        //	slotsX	is	used	as	a	shortcut	to	all	the	<slot>s
        //PT_XMLHashList slotsX = xml["slot"];
        IEnumerable<XElement> slotsX = root.Elements("slot");
        //for (int i = 0; i < slotsX.Count; i++)
        foreach( XElement slot in slotsX)
        {
            tSD = new SlotDef();    //	Create	a	new	SlotDef	instance
            if (slot.Attribute("type")!=null)
            {
                //	If	this	<slot>	has	a	type	 attribute	parse	it
                tSD.type = slot.Attribute("type").Value;
            }
            else
            {
                //	If	not,	set	its	type	to	"slot";	it's	a	card	in	the	rows
                tSD.type = "slot";
            }
            //	Various	attributes	are	parsed	into	numerical	values
            tSD.x = float.Parse(slot.Attribute("x").Value);
            tSD.y = float.Parse(slot.Attribute("y").Value);
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
            //	Sorting	Layers
            tSD.layerID = int.Parse(slot.Attribute("layer").Value);
            tSD.layerName = tSD.layerID.ToString(); //	b
                                                    //	pull	additional	attributes	based	on	the	type	of	each	<slot>
            switch (tSD.type)
            {
                case "slot":
                    //	ignore	slots	that	are	just	of	the	"slot"	type
                    break;
                case "drawpile":    //	c
                    tSD.stagger.x = float.Parse(slot.Attribute("xstagger").Value);
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":    //	d
                    tSD.player = int.Parse(slot.Attribute("player").Value);
                    tSD.rot = float.Parse(slot.Attribute("rot").Value);
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}

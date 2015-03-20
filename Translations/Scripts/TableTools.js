/*
 * File:        TableTools.js
 * Version:     1.0.1
 * CVS:         $Id$
 * Description: Copy, save and print functions for DataTables
 * Author:      Allan Jardine (www.sprymedia.co.uk)
 * Created:     Wed  1 Apr 2009 08:41:58 BST
 * Modified:    $Date$ by $Author$
 * Language:    Javascript
 * License:     LGPL
 * Project:     Just a little bit of fun :-)
 * Contact:     www.sprymedia.co.uk/contact
 * 
 * Copyright 2009 Allan Jardine, all rights reserved.
 *
 */

/*
 * Variable: TableToolsInit
 * Purpose:  Parameters for TableTools customisation
 * Scope:    global
 */
var TableToolsInit = {
	"oFeatures": {
		"bCsv": false,
		"bXls": true,
		"bCopy": true,
		"bPrint": true
	},
	"sPrintMessage": "",
	"sTitle": "",
	"iButtonHeight": 30,
	"iButtonWidth": 30,
	"_iNextId": 1 /* Internal useage - but needs to be global */
};


/*
 * Function: TableTools
 * Purpose:  TableTools "class"
 * Returns:  same as _fnInit
 * Inputs:   same as _fnInit
 */
function TableTools ( oInit )
{
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private parameters
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	var _oSettings;
	var nTools = null;
	var _nTableWrapper;
	var _aoPrintHidden = [];
	var _iPrintScroll = 0;
	var _nPrintMessage = null;
	var _DTSettings;
	var _sLastData;
	var _iId;
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Initialisation
	 */
	
	/*
	 * Function: _fnInit
	 * Purpose:  Initialise the table tools
	 * Returns:  node: - The created node for the table tools wrapping
 	 * Inputs:   object:oInit - object with:
 	 *             oDTSettings - DataTables settings
	 */
	function _fnInit( oInit )
	{
		_nTools = document.createElement('div');
		_nTools.className = "TableTools";
		_iId = TableToolsInit._iNextId++;
		
		/* Copy the init object */
		_oSettings = $.extend( true, {}, TableToolsInit );
		
		_DTSettings = oInit.oDTSettings;
		
		_nTableWrapper = fnFindParentClass( _DTSettings.nTable, "dataTables_wrapper" );

		return _nTools;
	}
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Support functions
	 */
	
	/*
	 * Function: fnGlue
	 * Purpose:  Wait until the id is in the DOM before we "glue" the swf
	 * Returns:  -
	 * Inputs:   object:clip - Zero clipboard object
	 *           node:node - node to glue swf to
	 *           string:id - id of the element to look for
	 *           string:text - title of the flash movie
	 * Notes:    Recursive (setTimeout)
	 */
	function fnGlue ( clip, node, id, text )
	{
		if ( document.getElementById(id) )
		{
			clip.glue( node, text );
		}
		else
		{
			setTimeout( function () {
				fnGlue( clip, node, id, text );
			}, 100 );
		}
	}
	
	
	/*
	 * Function: fnGetTitle
	 * Purpose:  Get the title of the page (from DOM or user set) for file saving
	 * Returns:  
	 * Inputs:   
	 */
	function fnGetTitle( )
	{
		if ( _oSettings.sTitle != "" )
			return _oSettings.sTitle;
		else
			return document.getElementsByTagName('title')[0].innerHTML;
	}
	
	
	/*
	 * Function: fnFindParentClass
	 * Purpose:  Parse back up the DOM to a node with a particular node
	 * Returns:  node: - found node
	 * Inputs:   node:n - Node to test
	 *           string:sClass - class to find
	 * Notes:    Recursive
	 */
	function fnFindParentClass ( n, sClass )
	{
		if ( n.className.match(sClass) || n.nodeName == "BODY" )
			return n;
		else
			return fnFindParentClass( n.parentNode, sClass );
	}
	
	
	/*
	 * Function: fnGetDataTablesData
	 * Purpose:  Get data from DataTables' internals and format it for output
	 * Returns:  
	 * Inputs:   
	 */
	function fnGetDataTablesData( sSeperator )
	{
		var i, iLen;
		var j, jLen;
		var sData = '';
		var sNewline = navigator.userAgent.match(/Windows/) ? "\r\n" : "\n";
		
		/* Titles */
		for ( i=0, iLen=_DTSettings.aoColumns.length ; i<iLen; i++ )
		{
			if (_DTSettings.aoColumns[i].bVisible && !_DTSettings.aoColumns[i].nTh.classList.contains("nExport") || _DTSettings.aoColumns[i].nTh.classList.contains("Export"))
			{
				sData += _DTSettings.aoColumns[i].sTitle.replace(/\n/g," ").replace( /<.*?>/g, "" ) +sSeperator;
			}
		}
		sData = sData.slice( 0, sSeperator.length*-1 );
		sData += sNewline;
		
		/* Rows */
		for ( j=0, jLen=_DTSettings.aiDisplay.length ; j<jLen ; j++ )
		{
			/* Columns */
			for ( i=0, iLen=_DTSettings.aoColumns.length ; i<iLen; i++ )
			{
				if (_DTSettings.aoColumns[i].bVisible && !_DTSettings.aoColumns[i].nTh.classList.contains("nExport") || _DTSettings.aoColumns[i].nTh.classList.contains("Export"))
				{
					sData += _DTSettings.aoData[ _DTSettings.aiDisplay[j] ]._aData[ i ].replace(/\n/g," ").replace( /<.*?>/g, "" ) +sSeperator;
				}
			}
			sData = sData.slice( 0, sSeperator.length*-1 );
			sData += sNewline;
		}
		
		/* Remove the last new line */
		sData.slice( 0, -1 );
		
		_sLastData = sData;
		return sData;
	}
	
	
	/* Initialise our new object */
	return _fnInit( oInit );
}


/*
 * Register a new feature with DataTables
 */
if ( typeof $.fn.dataTable == "function" && typeof $.fn.dataTableExt.sVersion != "undefined" )
{
	$.fn.dataTableExt.aoFeatures.push( {
		"fnInit": function( oSettings ) {
			return new TableTools( { "oDTSettings": oSettings } );
		},
		"cFeature": "T",
		"sFeature": "TableTools"
	} );
}
else
{
	alert( "Warning: TableTools requires DataTables 1.5 beta 9 or greater - "+
		"www.datatables.net/download");
}

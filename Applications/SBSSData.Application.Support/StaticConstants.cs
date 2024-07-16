﻿// Ignore Spelling: Linq

namespace SBSSData.Application.Support
{
    public static class StaticConstants
    {

        public static readonly string LocalStyles =
            """
            tr.columntotal {
                display:none;
            }

            body {
                padding:10px 20px 10px 20px;
                font-family: 'Segoe UI', Verdana, 'sans serif';
            }

            button.sbss {
                cursor:pointer; 
                font-weight:500; 
                font-size:12px;
                font-family:'Segoe UI Semibold', 'sans serif';
                border:1px solid black; 
                padding:2px 5px 5px 5px; 
                margin-right:10px;
                color:white;
                background-color: #4C74b2;
                border-radius: 15px;
                box-shadow: 2px 2px 10px 2px #aaaaaa;
            }

            div.overlay {
                position: fixed; 
                display: none; 
                width: 162px; 
                height: 360px;
                top: 200px;
                left: 245px;
                right: 0;
                bottom: 0;
                background-color: rgba(232,232,232,.75); 
                z-index: 9; 
                cursor: pointer; 
                border-radius:25px;
                border:2px solid firebrick;
                box-shadow:10px 10px 50px 15px #aaaaaa;
                padding:20px 25px 20px 25px;
            }

            a.help {
                float:right; 
                font-weight:500; 
                color:firebrick; 
                background-color:white; 
                padding-left:10px; 
                padding-right:10px" 
            }

             ul.sbss {
                 padding:0;
                 margin:0;
            }

            ul li {
                line-height:1.3;
                margin-bottom:10px
            }
            li.sbss{
                list-style-type: square;
                margin-top:0px;
                margin-bottom:0px;
                font-size:1.1em;
            }

            li::marker {
                color:firebrick;
            }

            span.showme {
                font-size:.75em; 
                cursor:pointer; 
            }

            div.help {
                display:none
            }

            a.navigate
            {
                font-size: .65em
            }

            body.playersheets
            {
                padding-top:0px;
            }

            h1.header
            {
                display: inline-block;
                font-weight: 500;
                font-size: 1.25em;
                color: firebrick;
            }

            div.season
            {
                padding-top: 10px;
                float: right;
                font-weight: 500;
                color: #4C74B2;
            }

            """;
        public static readonly string PlayerSheetsStyles =
            """
            body
            {
                padding-top:0px;
            }

            td.typeheader
            {
                width:680px;
            }   
            div.overlay
            {
            }

            h1.header
            {
                display: inline-block;
                font-weight: 500;
                font-size: 1.25em;
                color: firebrick;
            }

            div.season
            {
                padding-top: 10px;
                float: right;
                font-weight: 500;
                color: #4C74B2;
            }
            """;

        public static readonly string HelpStyles =
            """
            a.help {
                float: right;
                font-weight: 500;
                color: firebrick;
                background-color: white;
                padding-left: 10px;
                padding-right: 10px;
            }

            button.showme {
                background-color: #fffae6;
                border: 1px solid black;
                color: black;
                padding: 3px;
                text-align: center;
                text-decoration: none;
                display: inline-block;
                font-size: 12px;
                margin: 4px 2px;
                border-radius: 15px;
                box-shadow: 2px 2px 10px 2px #aaaaaa;
                font-size: 12px;
                height: 20px;
                width: 50px;
                padding: 0px;
                font-weight: 500;
            }

            span.showme {
                font-size: .75em;
                cursor: pointer;
            }

            div.help {
                display:none;
            }

            .tableTitle {
                font-style:italic;
            }
            """;

        public static readonly string SortableTableStyles =
            """
            table.sortable th {
                cursor:pointer;
            }
                 
            table.sortable th.ascending::before {
                content: "▲";
                margin-right:.25em;
                display:inline-block;
            }

            table.sortable th.descending::before {
                content: "▼";
                margin-right:.25em;
                display:inline-block;
            }

            table.sortable > thead > tr > td {
                background-color:#d62929;
                font-weight:400;
                color: white;
            }

            <!--table.sortable tfoot > tr:last-child td {
                font-weight:500;
                background-color:#ddd;
            }-->

            table.sortable>tfoot tr.summary  {
                font-weight:500;
                background-color:#ddd;
            }

            table.statistics > thead > tr > td {
                background-color:#d62929;
                font-weight:400;
                color: white;
            }
            """;

        public static readonly string LocalJavascript =
            """
            function toggleDetailsVisibility(detailsId, introductionId)
            {
                var sourceEl = document.getElementById("more");
                var details = document.getElementById(detailsId);
                var introduction = document.getElementById(introductionId);
                if (details.style.display === 'block')
                {
                    details.style.display = 'none';
                    introduction.style.display = "block";
                    sourceEl.innerText = 'More Details ↓ ...';

                }
                else
                {
                    details.style.display = 'block';
                    introduction.style.display = "none";
                    sourceEl.innerText = 'Less Details ↑ ...';
                }
            }

            function collapseTable(level)
            {
                var tables = document.getElementsByTagName("TABLE");
                for (i = 0; i < tables.length; i++)
                {
                    var table = tables[i];
                    if (table == null)
                    {
                        continue;
                    }
                    var updown = document.getElementById(table.id + 'ud');
                    if (updown == null) {
                        continue
                    }

                    var collapse = updown.className == "arrow-up";
                    var depth = updown.attributes["depth"].value;
                    if ((depth >= level) && collapse)
                    {
                        updown.className = "arrow-down";
                        if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith('sum'))
                        {

                            table.tHead.rows[1].style.display = 'none';
                        }

                        table.querySelector("tbody").style.display = "none";
                    }
                    else ((depth < level) && !collapse)
                    {
                        updown.className = "arrow-up";
                        if ((table.tHead.rows.length == 2) && !table.tHead.rows[1].id.startsWith('sum'))
                        {

                            table.tHead.rows[1].style.display = '';
                        }

                        table.querySelector("tbody").style.display = "none";
                        table.scrollIntoView
                        ({
                            behavior: 'smooth',
                            block: 'nearest'
                        });
                    }
                }
            }

            function viewAll(expandAll) {
                var tables = document.getElementsByTagName("TABLE");
                for (i = 0; i < tables.length; i++)
                {
                    var table = tables[i];
                    if (table == null)
                    {
                        continue;
                    }
                    var updown = document.getElementById(table.id + 'ud');
                    if (updown == null)
                    {
                        continue
                    }

                    if (expandAll)
                    {
                        updown.className = 'arrow-up';

                        table.style.borderBottomStyle = 'solid';
                        if (table.rows.length < 2 || table.tBodies.length == 0)
                        {
                            continue;
                        }

                        table.tBodies[0].style.display = ''; // expand ? '' : 'none';
                        if (table.tfoot != null)
                        {
                            table.tfoot.style.display = '';
                        }
                        if ((table.tHead.rows.length == 2) && !table.tHead.rows[1].id.startsWith('sum'))
                            table.tHead.rows[1].style.display = ''; // expand ? '' : 'none';
                    }
                    else
                    {
                        updown.className = 'arrow-down'; // collapse ? 'arrow-down' : 'arrow-up';

                        table.style.borderBottomStyle = 'dashed'; // collapse ? 'dashed' : 'solid';
                        if ((table.rows.length < 2) || (table.tBodies.length == 0))
                        {
                            continue;
                        }

                        table.tBodies[0].style.display = 'none'; // collapse ? 'none' : '';
                        if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith('sum'))
                            table.tHead.rows[1].style.display = 'none'; // collapse ? 'none' : '';
                        if (table.tFoot != null)
                            table.tFoot.style.display = 'none'; // collapse ? 'none' : '';
                    }
                }

                return false;
            }

            function expandTable(headerText)
            {
                var tableId = findTableIdFromHeader(headerText);
                if (tableId == -1)
                {
                    //alert("Can't find the table whose header starts with " + headerText);
                    return;
                }

                var table = document.getElementById(tableId);
                if (table != null)
                {
                    var updown = document.getElementById(tableId + 'ud');
                    if (updown != null)
                    {
                        updown.className = 'arrow-up';
                    }

                    table.style.borderBottomStyle = 'solid';
                    if (table.rows.length < 2 || table.tBodies.length == 0)
                    {
                        return
                    }

                    table.tBodies[0].style.display = '';
                    if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith('sum'))
                        table.tHead.rows[1].style.display = '';
                }
            }

            function collapseTable(headerText)
            {
                var tableId = findTableIdFromHeader(headerText);
                if (tableId == -1)
                {
                    //alert("Can't find the table whose header starts with " + headerText);
                    return;
                }

                var table = document.getElementById(tableId);
                if (table != null)
                {
                    var updown = document.getElementById(tableId + 'ud');
                    if (updown != null) 
                    {
                        updown.className = 'arrow-down';
                    }

                    table.style.borderBottomStyle = 'dashed';
                    if (table.rows.length < 2 || table.tBodies.length == 0)
                    {
                        return;
                    }

                    table.tBodies[0].style.display = 'none';
                    if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith('sum'))
                        table.tHead.rows[1].style.display = 'none';
                    if (table.tFoot != null)
                        table.tFoot.style.display = 'none';
                }
            }

            function findTableIdFromHeader(headerText) {
                var headers = document.querySelectorAll("a.typeheader")
                var tableId = -1;

                // This function is used to find table IDs of the first few tables so they can be expanded by the
                // introductory text. On the tables in the first league need be queried.
                for (let i = 0; i < headers.length; i++) {
                    let headerElement = headers[i];
                    //console.log(headerElement.innerText);
                    let header = headerElement.innerText;
                    //console.log("header = " + header);
                    if (header.startsWith(headerText)) {
                        tableId = headerElement.closest('table').id;
                        break;
                    }
                }

                return tableId
            }

            function getTableNestingLevel(element) {
                console.info("Checking nesting level for " + element.id);
                let nestingLevel = 0;
                let currentElement = element;

                while (currentElement.parentElement) {
                    console.info("Parent element is " + currentElement.parentElement.tagName)
                    if (currentElement.parentElement.tagName === 'TABLE') {
                        nestingLevel++;
                    }

                    currentElement = currentElement.parentElement;
                }

                return nestingLevel;
            }

            function findTableId(depth, index) {
                console.log("Finding table ID for depth=" + depth + " and index=" + index);
                var tables = document.getElementsByTagName("TABLE");
                var spansInHeader = document.querySelectorAll("a.typeheader span")
                var tableId = -1;
                for (let i = 0; i < spansInHeader.length; i++) {
                    var span = spansInHeader[i];
                    var tableDepth = span.getAttribute("depth");
                    var tableIndex = span.getAttribute("index");
                    if ((tableDepth == depth) && (tableIndex == index)) {
                        tableId = span.closest('table').id;
                        break;
                    }
                }

                return tableId
            }

            function toggleHelp(id) {
                var element = document.getElementById(id);
                var display = getComputedStyle(element).getPropertyValue("display");

                //console.log('in toggleHele for id=', id, " display=", display);
                element.style.display = (display == "none") ? "block" : "none";
            }

            function toggleStatisticsDisplay(buttonEl) { 
                var statistics = document.getElementsByClassName("statistics");
                var displayStyle = "";
                for (i = 0; i < statistics.length; i++)
                {
                    var statisticsCell = statistics[i].parentElement;
                    var display = window.getComputedStyle(statisticsCell).getPropertyValue("display")
                    displayStyle = (display == "none") ? "table-cell" : "none";
                    statisticsCell.style.display = displayStyle;
                }
            
                buttonEl.innerHTML = displayStyle == "table-cell" ? "Hide Statistics" : "Display Statistics";
                console.log(buttonEl.innerHtml + " for " + buttonEl.tagName);
                return displayStyle;
            }
            
            """;

        public static readonly string HelpJavascript =
            """
            function toggleHelp(id)
            {
                var element = document.getElementById(id);
                var display = getComputedStyle(element).getPropertyValue("display");

                console.log('in toggleHelp for id=', id, " display=", display);
                element.style.display = (display == "none") ? "block" : "none";
            }

            function openHelp(id)
            {
                var element = document.getElementById(id);
                if ((element !== "") && (element != null))
                {
                    element.style.display = "block";
                }
                else
                {
                    console.log("In openHelp, cannot find element for id=[" + id + "]");
                }
            }

            function closeHelp(id)
            {
                var element = document.getElementById(id);
                if ((element !== "") && (element != null))
                {
                    element.style.display = "none";
                }
                else
                {
                    console.log("In closeHelp, cannot find element for id=[" + id + "]");
                }
                element.style.display = "none";
            }

            function closeAllHelpButMe(id)
            {
                var helpItems = document.querySelectorAll("div.spacer.help")
                for (var i = 0; i < helpItems.length; i++)
                {
                    var helpItem = helpItems[i];
                    var itemId = helpItem.id;

                    if (itemId != id)
                    {
                        closeHelp(itemId);
                    }
                }
            }
            """;

        public static readonly string SortableTable =
            """
            <script language="javascript" type="text/javascript">
                (function setEventHandler(columnIndex, tableSelector, stringElements)
                {
                    const sortableTable = document.querySelector(tableSelector);
                    const tbody = sortableTable.querySelector("tbody");
                    const thead = sortableTable.querySelector("thead");

                    // Only rows in the body are sorted
                    const trs = [...tbody.querySelectorAll('tr')];
                    var headerCount = sortableTable.rows[1].cells.length;
                    const sortTypes = {};
                    for (var i = 0; i < headerCount; i++)
                    {
                        var isString = stringElements.indexOf(i) != -1;
                        sortTypes[i] = isString ? 1 : -1; 
                    };

                    let lastIndex = columnIndex - 1;
                    //const stringCols = [1];
                    let className = sortTypes[columnIndex - 1] < 0 ? "descending" : "ascending";
                    let selector = `tr th:nth-child(${columnIndex})`;
                    console.log("selector " + selector);
                    let activeHeader = thead.querySelector(selector);
                    console.log("Active Header" + activeHeader);
                    if (activeHeader != null)
                    {
                        activeHeader.classList.add(className);
                        console.log("Active Header " + activeHeader.outerHTML);

                        let sortTypeValue = !stringElements.includes(lastIndex) ? -1 : 1;
                        sortTypes[lastIndex] *= -1;
                    }

                    sortableTable.addEventListener('click', ({ target }) => 
                    {
                        if (!target.matches('th')) return;

                        const thIndex = Array.prototype.indexOf.call(target.parentElement.children, target);
                        console.log("lastIndex = " + lastIndex + " thIndex = " + thIndex);

                        const isNumeric = !stringElements.includes(thIndex);

                        //if (lastIndex != -1)
                        {
                            target.parentElement.children[lastIndex].removeAttribute('class');
                            if (lastIndex != thIndex)
                            {
                                var sortTypeValue = isNumeric ? -1 : 1;
                                sortTypes[thIndex] = sortTypeValue;
                            }
                        }

                        lastIndex = thIndex;

                        let getText = tr => tr.children[thIndex].textContent;
                        if (!isNumeric)
                        {
                           getText = tr => {
                              var fullName = tr.children[thIndex].textContent;
                              const nameParts = fullName.split(" ");

                              // Extract the first name (index 0) and the last name (all other parts)
                              const firstName = nameParts[0];
                              const lastName = nameParts.slice(1).join(" "); // Join the remaining parts

                              // Combine the last name and first name with a comma
                              return `${lastName}, ${firstName}`;
                           }
                        }

                        var className = sortTypes[thIndex] < 0 ? "descending" : "ascending";
                        target.classList.add(className);

                        //var rows = tbody.rows;
                        //var lastRowIndex = rows.length - 1;
                        //var summary = rows[lastRowIndex];
                        //tbody.deleteRow(lastRowIndex);
                        //summary.classList.add("summary");

                        trs.sort((a, b) => 
                        (
                            getText(a).localeCompare(getText(b), undefined, { numeric: isNumeric }) * sortTypes[thIndex]
                        ));

                        trs.forEach(tr => tbody.appendChild(tr));
                        //tbody.appendChild(summary);
                        sortTypes[thIndex] *= -1;
                    });
                })([[columnIndex]], [[tableSelector]], [[stringElements]]);
            
            </script>
            """;
        public static string OverlayTemplate =
                                     """
                                      <div id="overlay" class="overlay">
                                          <div id="overlayImage" style="text-align:center;">
                                              <img style="margin:auto; height:250px;" src="[[imagePath]][[imageName]].jpg"/>
                                          </div>
                                              <div id="overlayRankTable" style="padding-left:0px; width:160px; margin-top:5px;">[[infoHtml]]</div>
                                          </div>
                                      </div>
                                      """;
        public static string BuildOverlay(string imagePath, string imageName, string infoHtml) => OverlayTemplate.Replace("[[imagePath]]", imagePath)
                                                                                                                 .Replace("[[imageName]]", imageName)
                                                                                                                 .Replace("[[infoHtml]]", infoHtml);
        
        public static List<string> Seasons = ["2024 Summer", "2024 Spring", "2024 Winter", "2023 Fall", "2023 Summer"];

        //private static string helpNodeHtml = """
        //                                      <a class="help"
        //                                         onclick="getElementById('helpOverlay').style.display='block';">Help</a>
        //                                    """;

    }
}

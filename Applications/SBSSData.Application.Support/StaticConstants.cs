// Ignore Spelling: Linq

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
            button.sbss:hover, .button.sbss:focus {
                background-color: #829fc9;
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
            button.showme:hover, .button.showme:focus 
            {
                background-color: #dbe4f0;
            };

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

        public static readonly string AboutTablesStyles =
            """
            h1, h2, h3, h4, h5, h6
            {
                font-weight: 400;
            }

            h1
            {
                color:firebrick; 
                font-size:1.6em;
            }

            h2
            {
                color:#4C74B2;
                font-size:1.2em;
                font-weight:500;
            }

            h3
            {
                color: #4C74B2;
                font-size: 1.1em;
            }

            li
            {
                margin-bottom: 10px;
            }

            ul li
            {
                line-height: 1.3;
                margin-bottom: 10px
            }

            ul.sbss
            {
                padding: 0;
                margin-left: 20px;
            }
                ul.sbss li
                {
                    list-style-type: square;
                    margin-top: 0px;
                    margin-bottom: 10px;
                }

            li::marker
            {
                color: firebrick;
            }
                        
            a, a:visited, a:hover, a:visited:hover
            {
                font-weight:400;
            }

                a.navigate
                {
                    font-size: .65em
                }

            span.highlight
            {
                font-weight:500; 
                margin-left:.3em;
                margin-right:.3em; 
                padding-left:.3em; 
                padding-right:.3em
            }

            span.headerText
            {
                color:white; 
                background-color:firebrick
            }

            span.columnHeader
            {
                color:black; 
                background-color:#ddd
            }
            
            """;
        public static readonly string SortableTableStyles =
            """
            table.sortable th {
                cursor:pointer;
                white-space:nowrap;
            }

            table.sortable tbody tr td:nth-child(2) {
                white-space: nowrap;
            }
                 
            table.sortable th.ascending::before {
                content: "▲";
                display:inline-block;
            }

            table.sortable th.descending::before {
                content: "▼";
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

            .hidden {
                display:none;
            }

            table.sortable tr td.zscore {
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
                        if (table.tFoot != null)
                        {  
                            table.tFoot.style.display = '';
                            table.tFoot.firstElementChild.display = '';
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
                //console.log("Finding table ID for depth=" + depth + " and index=" + index);
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

            function toggleStatisticsDisplay(buttonEl) 
            { 
                var statistics = document.getElementsByClassName("statistics");
                var displayStyle = "";
                for (i = 0; i < statistics.length; i++)
                {
                    var statisticsCell = statistics[i].parentElement;
                    //var display = window.getComputedStyle(statisticsCell).getPropertyValue("display")
                    //displayStyle = (display == "none") ? "table-cell" : "none";
                    //statisticsCell.style.display = displayStyle;
                    statisticsCell.classList.toggle("hidden");
                }

                var sortableTables = document.getElementsByClassName("sortable");

                //console.log("Number of sortable tables is "+ sortableTables.length);

                for (var t = 0; t < sortableTables.length; t++)
                {
                    var table = sortableTables[t];
                    var headerCells = table.querySelectorAll("th");
                    var zScoreHeaders = table.querySelectorAll("th.zScore");
                    //console.log("Number zScoreHeaders = " + zScoreHeaders.length);
                    for (i = 0; i < zScoreHeaders.length; i++)
                    {
                        var columnIndex = zScoreHeaders[i].cellIndex;           // Get the column index

                        //console.log("Column index for "+ zScoreHeaders[i].innerHTML + " = " + columnIndex);
                        
                        var rows = Array.from(table.tBodies[0].rows).concat(Array.from(table.tFoot.rows)); 

                        //console.log("number of rows = " + rows.length);

                        for (var j = 0; j < rows.length; j++) 
                        {
                            var cell = rows[j].cells[columnIndex]; 
                            cell.classList.toggle("hidden");
                        } 


                        headerCells[columnIndex].classList.toggle("hidden");

                    }
                }
                
                var buttonText = buttonEl.innerHTML;
                buttonEl.innerHTML = (buttonText == "Hide Statistics") ? "Display Statistics" : "Hide Statistics";
                return displayStyle;
            }
            
            """;

        public static readonly string HelpJavascript =
            """
            function toggleHelp(id)
            {
                var element = document.getElementById(id);
                var display = getComputedStyle(element).getPropertyValue("display");

                //console.log('in toggleHelp for id=', id, " display=", display);
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
                    //console.log("In openHelp, cannot find element for id=[" + id + "]");
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
                    const trs = [...tbody.rows];

                    //console.log("Before setting event hander, number of rows = " + trs.length);

                    var headerCount = sortableTable.rows[1].cells.length;
                    const sortTypes = {};
                    for (var i = 0; i < headerCount; i++)
                    {
                        var isString = stringElements.indexOf(i) != -1;
                        sortTypes[i] = isString ? 1 : -1; 
                    };

                    let lastIndex = columnIndex - 1;
                    let className = sortTypes[columnIndex - 1] < 0 ? "descending" : "ascending";
                    let selector = `tr th:nth-child(${columnIndex})`;
                    //console.log("selector " + selector);
                    let activeHeader = thead.querySelector(selector);
                    // console.log("Active Header" + activeHeader);
                    if (activeHeader != null)
                    {
                        activeHeader.classList.add(className);
                        //console.log("Active Header " + activeHeader.outerHTML);

                        let sortTypeValue = !stringElements.includes(lastIndex) ? -1 : 1;
                        sortTypes[lastIndex] *= -1;
                    }

                    sortableTable.addEventListener('click', ({ target }) => 
                    {
                        if (!target.matches('th')) return;

                        const thIndex = Array.prototype.indexOf.call(target.parentElement.children, target);
                        //console.log("lastIndex = " + lastIndex + " thIndex = " + thIndex);

                        const isNumeric = !stringElements.includes(thIndex);

                        //if (lastIndex != -1)
                        {
                            target.parentElement.children[lastIndex].classList.remove('ascending', 'descending');
                            if (lastIndex != thIndex)
                            {
                                var sortTypeValue = isNumeric ? -1 : 1;
                                sortTypes[thIndex] = sortTypeValue;
                            }
                        }

                        lastIndex = thIndex;
                        getText = tr => {
                           //var children = tr.children[thIndex];

                           //console.log("In getText, for thIndex = " + thIndex + " number of children in the row is " + tr.children.length);
                           //console.log("First child is " + tr.children[0].outerHTML);

                           var fullName = tr.children[thIndex].firstChild.textContent.trim();
                           const nameParts = fullName.split(" ");
                      
                           // Extract the first name (index 0) and the last name (all other parts)
                           const firstName = nameParts[0];
                           const lastName = nameParts.slice(1);; // Join the remaining parts
                      
                           // Combine the last name and first name with a comma
                           return `${lastName}, ${firstName}`;
                        }
                        //}

                        var className = sortTypes[thIndex] < 0 ? "descending" : "ascending";
                        target.classList.add(className);

                        trs.sort(function(rowA, rowB) 
                        {
                            //console.log("In sort, isNumeric = " + isNumeric + " and thIndex = " + thIndex);
                            if (isNumeric)
                            {
                                return (rowA.children[thIndex].firstChild.textContent.trim() - rowB.children[thIndex].firstChild.textContent.trim()) * sortTypes[thIndex];
                            }
                            else
                            { 
                                return getText(rowA).localeCompare(getText(rowB)) * sortTypes[thIndex];
                            }
                        });

                        trs.forEach(tr => tbody.appendChild(tr));
                        sortTypes[thIndex] *= -1;
                    });
                })([[columnIndex]], [[tableSelector]], [[stringElements]]);
            
            </script>
            """;

        public static readonly string OverlayGenericTemplate =
                                    """
                                      <div id="overlay" class="overlay" style="position:fixed; [[overlayStyle]]">
                                          <div id="overlayImage">
                                              <img style="margin:auto; height:250px;" src="[[imagePath]][[imageName]].jpg"/>
                                          </div>
                                            <div id="overlayInfo" style="padding-left:0px;  
                                                                         max-width:160px; 
                                                                         margin-top:5px;  
                                                                         white-space:normal; 
                                                                         font-weight:400;
                                                                         background-color:#f2f2f2;
                                                                         border:solid 1px #b22222;
                                                                         display:flex; 
                                                                         justify-content:center;">
                                               <div style=""width:160px; text-align:center; \">
                                                    [[infoHtml]]
                                               </div>
                                            </div>   
                                          </div>
                                      </div>
                                      """;

        public static string BuildGenericOverlay(string overlayStyle,
                                                 string imagePath, 
                                                 string imageName, 
                                                 string infoHtml) => 
                                                 OverlayGenericTemplate.Replace("[[overlayStyle]]", overlayStyle)
                                                                       .Replace("[[imagePath]]", imagePath)
                                                                       .Replace("[[imageName]]", imageName)
                                                                       .Replace("[[infoHtml]]", infoHtml);

        public static readonly string OverlayTemplate =
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

        public static readonly string OverlayStatsTemplate =
                                      """
                                      <div id="overlay" class="overlay" style="height:385px">
                                          <div id="overlayImage" style="text-align:center;">
                                              <img style="margin:auto; height:250px;" src="[[imagePath]][[imageName]].jpg"/>
                                          </div>
                                              <div id="information" style="padding-left:0px; width:160px; margin-top:5px; text-align:center;">
                                                <table style="display:inline-table; margin:auto; background-color:rgba(255,255,255,1.0);">
                                                    <thead>
                                                        <tr><th colspan="3" style="text-align:center">[[FirstName]]'s Z-Scores</th>
                                                        <tr><th title="Stat name">Stat</th>
                                                            <th title="[[Name]]'s stat value" style="text-align:right;">Value</th>
                                                            <th title="[[Name]]'s Z-Score">Z-Score</th>
                                                    </thead>
                                                    <tbody>
                                                        <tr><td>AVE</td><td style="text-align:right;">[[V0]]</td><td>[[Z0]]</td></tr>
                                                        <tr><td>SLG</td><td style="text-align:right;">[[V1]]</td><td>[[Z1]]</td></tr>
                                                        <tr><td>OBP</td><td style="text-align:right;">[[V2]]</td><td>[[Z2]]</td></tr>
                                                        <tr><td>OPS</td><td style="text-align:right;">[[V3]]</td><td>[[Z3]]</td></tr>
                                                    </tbody>
                                                </table>
                                              </div>
                                          </div>
                                      </div>
                                      """;
        public static string BuildOverlayStats(string imagePath,
                                               string imageName,
                                               string firstName,
                                               string name,
                                               List<string> statValues,
                                               List<string> zScores)
        {
            string altered = OverlayStatsTemplate.Replace("[[imagePath]]", imagePath)
                                                 .Replace("[[imageName]]", imageName)
                                                 .Replace("[[FirstName]]", firstName)
                                                 .Replace("[[Name]]", name);
            for (int i = 0; i < 4; i++)
            {
                altered = altered.Replace($"[[Z{i}]]", zScores[i]).Replace($"[[V{i}]]", statValues[i]);
            }

            return altered;
        }

        public static readonly List<string> Seasons = ["2025 Winter", "2024 Fall", "2024 Summer", "2024 Spring", "2024 Winter", "2023 Fall", "2023 Summer"];

        //private static string helpNodeHtml = """
        //                                      <a class="help"
        //                                         onclick="getElementById('helpOverlay').style.display='block';">Help</a>
        //                                    """;

    }
}

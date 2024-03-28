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

function viewAll(expandAll) 
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
            if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith('sum'))
                table.tHead.rows[1].style.display = ''; // expand ? '' : 'none';
        }
        else
        {
            updown.className = 'arrow-down'; // collapse ? 'arrow-down' : 'arrow-up';

            table.style.borderBottomStyle = 'dashed'; // collapse ? 'dashed' : 'solid';
            if (table.rows.length < 2 || table.tBodies.length == 0)
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
        alert("Can't find the table whose header starts with " + headerText);
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
        alert("Can't find the table whose header starts with " + headerText);
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

function findTableIdFromHeader(headerText) 
{
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

function getTableNestingLevel(element) 
{
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

function findTableId(depth, index) 
{
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
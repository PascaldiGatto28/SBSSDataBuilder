function toggle(id)
{
    var table = document.getElementById(id);
    if (table == null) return false;
    var updown = document.getElementById(id + 'ud');
    if (updown == null) return false;
    var expand = updown.className == 'arrow-down';
    updown.className = expand ? 'arrow-up' : 'arrow-down';
    table.style.borderBottomStyle = expand ? 'solid' : 'dashed';
    if (table.rows.length < 2 || table.tBodies.length == 0) return false;
    table.tBodies[0].style.display = expand ? '' : 'none';
    if (table.tHead.rows.length == 2 && !table.tHead.rows[1].id.startsWith ('sum'))
    table.tHead.rows[1].style.display = expand ? '' : 'none';
    if (table.tFoot != null)
    table.tFoot.style.display = expand ? '' : 'none';
    if (expand)
    table.scrollIntoView ({behavior:'smooth', block:'nearest'});
    return false;
}

function insertTodaysDate() {
    const spanElement = document.getElementById("todaysDate");
    var options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    var today = new Date();
    formattedDate = today.toLocaleDateString("en-US", options); 
    spanElement.innerHTML = formattedDate;
}

function convertYYDDDtoDate() 
{
    var input = document.getElementById("version").innerHTML.trim().substring(13, 18);
    
    // Parse the input string
    const year = parseInt(input.substring(0, 2));
    const dayOfYear = parseInt(input.substring(2));

    // Calculate the date
    const baseDate = new Date(year + 2000, 0, 1); 
    const targetDate = new Date(baseDate.getTime() + (dayOfYear - 1) * 24 * 60 * 60 * 1000);

    // Get the day of the week and month
    const daysOfWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    console.log(daysOfWeek);
    const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    console.log(months);
    const dayOfWeek = daysOfWeek[targetDate.getDay()];
    console.log(dayOfWeek);
    const month = months[targetDate.getMonth()];
    console.log(month);

    // Format the result
    return dayOfWeek+" "+month+" "+(targetDate.getDate())+", "+(targetDate.getFullYear());
}


//window.onload = function ()
//{
//    var formattedDate = convertYYDDDtoDate();
//    document.getElementById("releaseDate").innerHTML = "Release Date " + formattedDate;
//}

//When the user clicks on the button, toggle between hiding and showing the dropdown content
function myFunction() 
{
    var el = document.getElementById("myDropdown").classList;
    document.getElementById("myDropdown").classList.toggle("show");
}

// Close the dropdown if the user clicks outside of it
window.onclick = function (event) 
{
    if (!event.target.matches('.dropbtn')) 
    {
        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
          for (i = 0; i < dropdowns.length; i++) 
          {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) 
            {
                openDropdown.classList.remove('show');
            }
          }
    }
}

function toggleDetailsVisibility(detailsId)
{
    var sourceEl = document.getElementById("more");
    var details = document.getElementById(detailsId);
    if (details.style.display === 'block')
    {
        details.style.display = 'none';
        //introduction.style.display = "block";
        sourceEl.innerText = 'More Release Notes ↓ ...';

    }
    else
    {
        details.style.display = 'block';
        sourceEl.innerText = 'Latest Release Notes ↑ ...';
    }
}

    


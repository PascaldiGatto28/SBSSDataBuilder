// Ignore Spelling: Linq

namespace SBSSData.Application.LinqPadQuerySupport
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
            }

            div.overlay {
                position: fixed; 
                display: none; 
                width: 256px; 
                height: 486px;
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
                padding:25px;
            }
        """;

        public static string OverlayTemplate(string imagePath = "", string imageName = "", string rankTable = "") =>
                                     $"""
                                      <div id="overlay" class="overlay">
                                          <div id="overlayImage" style="text-align:center;">
                                              <img style="margin:auto;" src="{imagePath}{imageName}.jpg"/>
                                          </div>
                                              <div id="overlayRankTable" style="padding-left:22px; width:220px; margin-top:10px;">{rankTable}</div>
                                          </div>
                                      </div>
                                      """;
    }
}

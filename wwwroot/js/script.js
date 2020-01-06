$(document).ready(function () {
    $("#departure").keyup(function () {
        var text = $(this).val();
        var url = "https://www.expedia.com/api/v4/typeahead/" + text + "?callback=cb_1577712281098_108960&client=Flights.Search&siteid=1&guid=3cb7cb76df844189a89630639e50e240&lob=FLIGHTS&locale=en_US&expuserid=-1&regiontype=95&ab=&dest=false&maxresults=10&features=uta_client%7Cnearby_airport%7Cta_hierarchy&format=jsonp&device=Desktop&browser=Chrome&personalize=false";
        $("#departure_result").html("");
        $("#arrival_result").html("");
        if (text == "")
            return;
        $.get(url, function (data) {
            var list = data.split('"q"');
            var json_string = '{"q"' + list[1];
            json_string = json_string.slice(0, json_string.length - 1);

            var search_result = JSON.parse(json_string);
            var flight_list = search_result.sr;
            var i;
            for (i = 0; i < flight_list.length && i < 10; i++) {
                var append_html = '<li id="' + flight_list[i].hierarchyInfo.airport.airportCode
                    + '"><a href="#">' + flight_list[i].regionNames.displayName + '</a></li>';
                $("#departure_result").append(append_html);
                
            }
        });
    });
    $("#arrival").keyup(function () {
        var text = $(this).val();
        var url = "https://www.expedia.com/api/v4/typeahead/" + text + "?callback=cb_1577712281098_108960&client=Flights.Search&siteid=1&guid=3cb7cb76df844189a89630639e50e240&lob=FLIGHTS&locale=en_US&expuserid=-1&regiontype=95&ab=&dest=false&maxresults=10&features=uta_client%7Cnearby_airport%7Cta_hierarchy&format=jsonp&device=Desktop&browser=Chrome&personalize=false";
        $("#departure_result").html("");
        $("#arrival_result").html("");
        if (text == "")
            return;
        $.get(url, function (data) {
            var list = data.split('"q"');
            var json_string = '{"q"' + list[1];
            json_string = json_string.slice(0, json_string.length - 1);

            var search_result = JSON.parse(json_string);
            var flight_list = search_result.sr;

            var i; 
            for (i = 0; i < flight_list.length && i < 10; i++) {
                var append_html = '<li id="' + flight_list[i].hierarchyInfo.airport.airportCode
                    + '"><a href="#">' + flight_list[i].regionNames.displayName + '</a></li>';
                $("#arrival_result").append(append_html);

            }
        });
    });
    $("body").on("click", "#departure_result li", function () {
        $("#departure").val($(this).attr("id"));
    });
    $("body").on("click", "#arrival_result li", function () {
        $("#arrival").val($(this).attr("id"));
    });
    $("body").click(function () {
        $("#departure_result").html("");
        $("#arrival_result").html("");
    });
});
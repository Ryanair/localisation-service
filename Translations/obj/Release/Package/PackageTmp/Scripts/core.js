Namespace('FR');
Namespace('FR.Translations');

FR.Translations = {
	Init: function () {
		$("legend.langHeader").click(function () {
			if ($(this).next("div.langDescription").hasClass("hide")) {
				$(this).next("div.langDescription").removeClass("hide");
				$(this).parent("fieldset").addClass("active");
			}
			else {
				$(this).next("div.langDescription").addClass("hide");
				$(this).parent("fieldset").removeClass("active");
			}
		});
		$("input#pluralKey").click(function () {

			if ($(this).is(':checked')) {
				$(this).parent().find("input.pluralInput").removeClass("hide");
				$(this).attr("checked", "checked");
			}
			else {
				$(this).parent().find("input.pluralInput").val("");
				$(this).parent().find("input.pluralInput").addClass("hide"); $(this).removeAttr("checked", "checked");
			}
		});
		$("a.validateTrans").click(function (e) {
			e.preventDefault();
			var keyValue = $("input#TransKey").val();
			$.ajax({
				url: "validateTrans",
				type: "POST",
				data: JSON.stringify({ 'Key': keyValue }),
				dataType: "json",
				traditional: true,
				contentType: "application/json; charset=utf-8",
				success: function (data) {
					if (data.status == "create") {
						$(".tagContainer").removeClass("hide");
						$(".langContainer").removeClass("hide");
						$(".buttonsContainer").removeClass("hide");
						//$(".editTranslation").addClass("hide")
					} else {
						window.location.href = data.html;
						//$(".tagContainer").addClass("hide");
						//$(".langContainer").addClass("hide");
						//$(".editTranslation").removeClass("hide")
					}
				},
				error: function () {
					alert("An error has occured!!!");
				}
			});




		});
		$(".checkListTags li input").click(function (e) {
			if ($(this).is(':checked')) {
				$(this).attr("checked", "checked");
			}
			else { $(this).removeAttr("checked", "checked"); }
		});
		$("div.addline").click(function () {
			var $html = $(this).next().find(".arrayLineToCopy").clone(true, true);
			$html.attr("class", "arrayLine");
			$(this).next().append($html);
		});
		$("div.removeLine").click(function () {
			if ($(this).hasClass("edit")) {
				$(this).parent(".arrayLine").find("input#transArray").val("");
			}
			else {
				$(this).parent(".arrayLine").remove();
			}
			
		});
		$("table.transTable").dataTable({
			"iDisplayLength": 20,
			"sPaginationType": "full_numbers",
			"bFilter": true,
			"bLengthChange": false,
			"columns": [{ "sortable": true }, { "sortable": true }, { "sortable": true }, { "sortable": false }],
			"sDom": '<"clear"><"tableFilter"f>lrtip'
		});
		$(".checkbox").click(function () {
			var $inputC = $(this).parent().find("input[type='checkbox']");
			if ($inputC.is(':checked')) {
				$inputC.removeAttr("checked", "checked");
				
			}
			else { $inputC.attr("checked", "checked"); }
		});
		$("#transLangdrop").change(function () {
			var language = $(this).find('option:selected').val();
			$.post("/Translations",
				{
					lang: language
				}, function (data) {
					$(".renderPartial").html(data);

					$("table.transTable").dataTable({
						"iDisplayLength": 20,
						"sPaginationType": "full_numbers",
						"bFilter": true,
						"bLengthChange": false,
						"columns": [{ "sortable": true }, { "sortable": true }, { "sortable": true }, { "sortable": false }],
						"sDom": '<"clear"><"tableFilter"f>lrtip'
					});
				});
		});
		$(".FRDatepickerDefault").datepicker({
			dateFormat: "yy-mm-dd"
		});
		$("form.createTransForm").submit(function (event) {
			//fix Tags
			var tagIdx = 0;

			$('form.createTransForm fieldset:not(.active)').remove();

			$("input[id='tagKey'][checked='checked']").each(function () {
				$(this).attr("name", "Tags[" + tagIdx + "].TagKey");
				tagIdx++;
			});

			var langIdx = 0;
			$("fieldset.active").each(function () {
				$(this).find("input[id='TransDescription']").each(function () {
					$(this).attr("name", "TransLang[" + langIdx + "].TransDescription");
				});

				$(this).find("input[id='langKey']").each(function () {
					$(this).attr("name", "language[" + langIdx + "].LangKey");
				});

				$(this).find("input[id='langText']").each(function () {
					$(this).attr("name", "language[" + langIdx + "].LangText");
				});

				$(this).find("input[id='langIsActive']").each(function () {
					$(this).attr("name", "language[" + langIdx + "].IsActive");
				});
				
				var pluralIdx = 0;
				$(this).find("input[id='pluralKey']").each(function () {
					$(this).attr("name", "TransPlural[" + langIdx + "][" + pluralIdx + "].Plural.PluralKey");
					$(this).parent().find('.pluralInput').attr("name", "TransPlural[" + langIdx + "][" + pluralIdx + "].PluralDescription");
					pluralIdx++;
				});

				var arrayIdx = 0;
				$(this).find("input[id='transArray']").each(function () {
					$(this).attr("name", "transArray[" + langIdx + "][" + arrayIdx + "].ArrayDescription");
					arrayIdx++;
				});

				var arrayIdx = 0;
				$(this).find("input[id='transArrayId']").each(function () {
					$(this).attr("name", "transArray[" + langIdx + "][" + arrayIdx + "].TransArrayId");
					arrayIdx++;
				});

				langIdx++;
				
			});
		});

		//$(".btn").click(function () {
		//	$.isLoading({ text: "Loading wait" });
		//	// Setup Loading plugin
		//	$("#load-overlay .demo p").removeClass("alert-success");
		//	// Re-enabling event
		//	setTimeout(function () {
		//		$.isLoading("hide");
		//		$("#load-overlay .demo p").html("Content Loaded").addClass("alert-success");
		//	}, 2000);

		//});

		//$("form.formLoading").submit(function (event) {
		//	var formAction = $(this).attr("action");
		//	var formData = $(this).serialize();
		//	var formType = $(this).attr("method");

		//	$.isLoading({ text: "Loading wait" });
		//	//event.preventDefault();

		//	request = $.ajax({
		//		url: formAction,
		//		type: formType,
		//		data: formData,
		//		always: function () {
		//			$.isLoading("hide");
		//		}
		//	});
		//});



	},
};

$(document).ready(function () {
	FR.Translations.Init();
});

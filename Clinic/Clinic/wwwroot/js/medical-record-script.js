var globalSelectedServId = null;

function replaceBracketValues(str, ...values) {
    // Используем регулярное выражение для поиска скобок []
    const regex = /\[(.*?)\]/g;

    // Заменяем значения внутри скобок на переданные числа
    let result = str.replace(regex, (match) => {
        if (values.length > 0) {
            return `[${values.shift()}]`;
        } else {
            return match; // Если передано меньше чисел, оставляем значение без изменений
        }
    });

    return result;
}

//Функция, которая обновляет имена для service и его material
function updateNamesServiceMaterials() {
    var tableDestService = document.getElementById("table_dest_service");

    var counterServ = 0;
    var counterMat = 0;

    for (var i = 1; i < tableDestService.rows.length; i++) {
        var rowTableDestService = tableDestService.rows[i];

        counterMat = 0;

        //Получение ServId из таблицы сервисов
        var cellHiddenServIdInServ = rowTableDestService.cells[0];
        var inputElement_1 = cellHiddenServIdInServ.querySelector('input');
        var ServIdSer = inputElement_1.value;

        //Изменение имен для полей услуг
        var inputsService = rowTableDestService.querySelectorAll("input");
        inputsService.forEach(function (input) {
            var nameInput = input.name;
            input.name = replaceBracketValues(nameInput, (counterServ));
        });

        var select = rowTableDestService.querySelectorAll("select")[0];
        var nameSelect = select.name;
        select.name = replaceBracketValues(nameSelect, (counterServ));

        //Изменение имен для полей материалов
        var tableDestMaterial = document.getElementById("table_dest_material");
        for (var j = 1; j < tableDestMaterial.rows.length; j++) {
            var rowTableDestMaterial = tableDestMaterial.rows[j];

            //Получение ServId из таблицы материалов
            var ServIdMat = rowTableDestMaterial.cells[0].textContent;

            if (parseInt(ServIdSer) === parseInt(ServIdMat)) {
                var inputsMaterial = rowTableDestMaterial.querySelectorAll("input");
                inputsMaterial.forEach(function (input, index) {
                    if (index === inputsService.length) {
                        var valueInput = input.value;
                        input.value = replaceBracketValues(valueInput, counterServ, counterMat);
                    } else {
                        var nameInput = input.name;
                        input.name = replaceBracketValues(nameInput, counterServ, counterMat);
                    }
                });

                counterMat++;
            }
        }
        counterServ++;
    }
}

//Функция составления выпадающего списка докторов для выбранных услуг
function fillSelectOptions(ServId, DoctId) {
    var table = document.getElementById("table_dest_service");

    // Итерируемся по строкам таблицы, пропуская заголовок
    for (var i = 1; i < table.rows.length; i++) {
        var row = table.rows[i];

        var cellHiddenServId = row.cells[0];
        var inputElement = cellHiddenServId.querySelector('input');
        var hiddenServId = inputElement.value;

        if (hiddenServId && parseInt(hiddenServId) === parseInt(ServId)) {

            var select = row.querySelector(".select-worker");

            // AJAX запрос
            $.ajax({
                url: '/MedRec/CreateMedicalRecord?handler=WorkersByDoctor&DoctId=' + DoctId,
                type: 'GET',
                success: function (data) {
                    // Очищаем выпадающий список
                    select.innerHTML = '';

                    console.log(JSON.stringify(data));
                    // Добавляем полученные данные в выпадающий список
                    $.each(data, function (key, value) {
                        var option = document.createElement('option');
                        option.value = key;
                        option.text = value;
                        select.appendChild(option);
                    });
                },
                error: function (xhr, status, error) {
                    console.log(error);
                }
            });

        }
    }
}

//Функция, которая ситает всю сумму за услуги и материалы
function calculateAll() {
    var tableDestService = document.getElementById("table_dest_service");
    var rowsTableDestService = tableDestService.getElementsByTagName("tr");

    var sumService = 0;
    for (var i = 1; i < rowsTableDestService.length; i++) {
        var rowTableDestService = rowsTableDestService[i];

        sumService += parseFloat(rowTableDestService.cells[4].textContent);
    }

    var tableDestMaterial = document.getElementById("table_dest_material");
    var rowsTableDestMaterial = tableDestMaterial.getElementsByTagName("tr");

    var sumMaterial = 0;
    for (var i = 1; i < rowsTableDestMaterial.length; i++) {
        var rowTableDestMaterial = rowsTableDestMaterial[i];

        sumMaterial += parseFloat(rowTableDestMaterial.cells[5].textContent.replace(",", "."));
    }

    var inputsResult = document.getElementById("sum-service-material");
    inputsResult.value = calculateWithFormatting(parseFloat(sumService), parseFloat(sumMaterial), "+");
}

//Функция обновления обработчиков событий для полей с количеством услуг
//Обработчик события при смене значения в поле с количеством услуг меняет 
//общую сумму за услуги в таблице
function attachEventListenersChangeInputCountService() {
    var table = document.getElementById("table_dest_service");
    var rows = table.getElementsByTagName("tr");

    for (var i = 1; i < rows.length; i++) {
        var inputField = rows[i].cells[2];

        // Проверяем, что обработчик события еще не добавлен
        if (!inputField.hasAttribute("data-event-listener")) {
            inputField.addEventListener("change", function (event) {

                const row = event.target.closest('tr');

                var cellCount = row.cells[2];
                var inputElement = cellCount.querySelector('input');
                var Count = inputElement.value;

                var PriceServValue = row.cells[3].textContent;
                var Amount = parseInt(Count) * parseInt(PriceServValue);

                row.cells[4].textContent = Amount;

                calculateAll();
            });

            // Помечаем поле, чтобы указать, что обработчик события уже добавлен
            inputField.setAttribute("data-event-listener", "true");
        }
    }
}

//Функция обновления обработчиков событий для полей с количеством материалов
//Обработчик события при смене значения в поле с количеством материала меняет 
//общую сумму за материалы в таблице
function attachEventListenersChangeInputCountMaterial() {
    var table = document.getElementById("table_dest_material");
    var rows = table.getElementsByTagName("tr");

    for (var i = 1; i < rows.length; i++) {
        var inputField = rows[i].cells[3];

        // Проверяем, что обработчик события еще не добавлен
        if (!inputField.hasAttribute("data-event-listener")) {
            inputField.addEventListener("change", function (event) {

                const row = event.target.closest('tr');

                var cellCount = row.cells[3];
                var inputElement = cellCount.querySelector('input');
                var Count = inputElement.value;

                var PriceMatValue = row.cells[4].textContent;
                var Amount = calculateWithFormatting(parseFloat(Count), parseFloat(PriceMatValue), "*");

                row.cells[5].textContent = Amount;

                calculateAll();
            });

            // Помечаем поле, чтобы указать, что обработчик события уже добавлен
            inputField.setAttribute("data-event-listener", "true");
        }
    }
}

//Подсчет аргументов с форматированием
function calculateWithFormatting(a, b, operator) {
    let result;
    if (operator === "*") {
        result = (a * b).toFixed(2);
    } else if (operator === "+") {
        result = (a + b).toFixed(2);
    }

    if (result % 1 === 0) {
        result = parseInt(result);
    }

    result = result.toString().replace(".", ",");

    return result;
}

//Обработчик события изменения значения фильтров для table_source_service
function handleChangeSelectFiltersServices() {
    // Получаем ссылку на таблицу
    var tableSourse = document.querySelector('#table_source_service');

    // Получаем выбранные значения из обоих select
    var selectedDoctValue = document.getElementById("doctFilter").value;
    var selectedGrpValue = document.getElementById("grpFilter").value;

    // Итерируемся по строкам таблицы, пропуская заголовок
    for (var i = 1; i < tableSourse.rows.length; i++) {
        var row = tableSourse.rows[i];

        // Получаем значения каждой ячейки в текущей строке
        var flag = row.cells[0].textContent;
        var doctValue = row.cells[2].textContent;
        var grpValue = row.cells[3].textContent;

        // Проверяем соответствие значений фильтров в текущей строке
        if (flag === "True") {
            row.style.display = "none";
            continue;
        }

        var showRow =
            (parseInt(selectedDoctValue) === 0 || parseInt(selectedDoctValue) === parseInt(doctValue)) &&
            (parseInt(selectedGrpValue) === 0 || parseInt(selectedGrpValue) === parseInt(grpValue));

        // Устанавливаем видимость строки в зависимости от результата проверки
        row.style.display = showRow ? "" : "none";
    }
}

//Обработчик клика на кнопке "add-service"
function handleButtonAddService(event) {
    const row = event.target.closest('tr');

    if (!row) return; // Добавьте проверку, если строка не найдена

    const ServId = row.cells[1].textContent;
    const DoctId = row.cells[2].textContent;
    const ServName = row.cells[6].textContent;
    const PriceServValue = row.cells[9].textContent;

    row.cells[0].textContent = "True";
    row.style.display = "none";

    // Создание новой строки в таблице table_dest_service
    const tableDest = document.getElementById('table_dest_service');
    const newRow = document.createElement('tr');

    newRow.innerHTML = `
        <td class="col-id-hidden">
            <input hidden type="number" value="${ServId}" name="inputMedicalRecordModel.SelectedServices[*].ServId">
        </td>
        <td>${ServName}</td>
        <td><input type="number" step="1" min="1" value="1" name="inputMedicalRecordModel.SelectedServices[*].Count"></td>
        <td>${PriceServValue}</td>
        <td>${PriceServValue}</td>
        <td>
          <div>
            <select class="select-worker" name="inputMedicalRecordModel.SelectedServices[*].WorkId"></select>
          </div>
        </td>
        <td class="th-td-last">
          <div class="btn-wrapper-table">
            <a class="btn btn-wrapper-table-box del-service">
              <img src="/image/icon-delete.png" width="16px">
            </a>
            <a class="btn btn-wrapper-table-box show-materials">
                <img src="/image/icon-material.png" width="16px">
            </a>
          </div>
        </td>
    `;

    //Навешивание обработчика события на кнопку "del-service" в новой строке
    const deleteButtonDel = newRow.querySelector('.del-service');
    deleteButtonDel.addEventListener('click', handleDeleteService);

    //Навешивание обработчика события на кнопку "show-service" в новой строке
    const deleteButtonShow = newRow.querySelector('.show-materials');
    deleteButtonShow.addEventListener('click', handleShowMaterial);

    tableDest.querySelector('tbody').appendChild(newRow);

    attachEventListenersChangeInputCountService();
    fillSelectOptions(ServId, DoctId);

    updateNamesServiceMaterials() //Обновление имен для Сервисов
    calculateAll();
}

//Обработчик клика на кнопке "del-service"
function handleDeleteService(event) {
    const row = event.target.closest('tr');

    if (!row) return; // Добавьте проверку, если строка не найдена

    var cellHiddenServId = row.cells[0];
    var inputElement = cellHiddenServId.querySelector('input');
    var ServId = inputElement.value;

    //Скрыть блок с выбором материалов, если удаляеся услуга с открытым блоком
    if (parseInt(ServId) === parseInt(globalSelectedServId)) {
        document.getElementById("hidden-material-block").style.display = "none";
    }

    //Поиск в таблице всех услуг необходимой услуги (которая удаляется)
    var tableSourse = document.querySelector('#table_source_service');

    // Итерируемся по строкам таблицы, пропуская заголовок
    for (var i = 1; i < tableSourse.rows.length; i++) {
        var rowTableSourse = tableSourse.rows[i];

        if (parseInt(rowTableSourse.cells[1].textContent) === parseInt(ServId)) {
            rowTableSourse.cells[0].textContent = "False";
            rowTableSourse.style.display = "";

            row.remove();

            //Удаление всех материалов, относящихся к сервису
            var tableDestMaterial = document.getElementById("table_dest_material");

            for (var j = 1; j < tableDestMaterial.rows.length; j++) {
                var rowTableDestMaterial = tableDestMaterial.rows[j];

                var ServIdMat = rowTableDestMaterial.cells[0].textContent;

                //Удаление одной строки материалов
                if (parseInt(ServId) == parseInt(ServIdMat)) {
                    
                    //Извлечение MatId
                    var cellMatId = rowTableDestMaterial.cells[1];
                    var inputElement = cellMatId.querySelector('input');
                    var MatId = inputElement.value;

                    //Поиск в таблице всех услуг необходимой услуги (которая удаляется)
                    var tableSourseMaterial = document.querySelector('#table_source_material');

                    // Итерируемся по строкам таблицы, пропуская заголовок
                    for (var t = 1; t < tableSourseMaterial.rows.length; t++) {
                        var rowTableSourseMaterial = tableSourseMaterial.rows[t];

                        if (parseInt(rowTableSourseMaterial.cells[3].textContent) === parseInt(MatId)) {
                            rowTableSourseMaterial.cells[0].textContent = "False";
                            rowTableSourseMaterial.style.display = "";

                            rowTableDestMaterial.remove();
                            j--;

                            break;
                        }
                    }

                }
            }


            handleChangeSelectFiltersServices();
            updateNamesServiceMaterials() //Обновление имен для Сервисов

            break;
        }
    }
    calculateAll();
}

//Обработчик клика на кнопке "show-material"
function handleShowMaterial(event) {
    const row = event.target.closest('tr');
    if (!row) return; // Добавьте проверку, если строка не найдена

    var cellHiddenServId = row.cells[0];
    var inputElement = cellHiddenServId.querySelector('input');
    var ServId = inputElement.value;

    //Назначить ServId глобальной переменной выбранного сервиса
    //для редактирования набора материалов
    globalSelectedServId = ServId;

    //Поиск в таблице всех услуг необходимой услуги (о которой нужно получить информацию)
    var tableSourse = document.querySelector('#table_source_service');

    // Итерируемся по строкам таблицы, пропуская заголовок
    for (var i = 1; i < tableSourse.rows.length; i++) {
        var rowTableSourse = tableSourse.rows[i];

        if (parseInt(rowTableSourse.cells[1].textContent) === parseInt(ServId)) {

            var DoctName = rowTableSourse.cells[4].textContent;
            var StdServId = rowTableSourse.cells[5].textContent;
            var ServName = rowTableSourse.cells[6].textContent;
            var ServGrpName = rowTableSourse.cells[7].textContent;
            var ServTypeName = rowTableSourse.cells[8].textContent;
            var PriceServValue = rowTableSourse.cells[9].textContent;
            var ServDuration = rowTableSourse.cells[10].textContent;

            // Сделать видимым блок с id=hidden-material-block
            const hiddenBlock = document.getElementById('hidden-material-block');
            hiddenBlock.style.display = 'block';

            // Получение элементов для добавления данных из JSON-объекта
            const wrapperInfoService = document.querySelector('.wrapper-info-service');
            wrapperInfoService.innerHTML = '';

            // Создание и добавление p элементов внутри .info-service
            const infoService1 = document.createElement('div');
            infoService1.classList.add('info-service');
            infoService1.innerHTML = `
              <p><span class="selection-secondary">Специальность:</span> ${DoctName}</p>
              <p><span class="selection-secondary">Код по стандарту:</span> ${StdServId !== null ? StdServId : 'Нет'}</p>
              <p><span class="selection-secondary">Услуга:</span> ${ServName}</p>
              <p><span class="selection-secondary">Группа:</span> ${ServGrpName}</p>
            `;
            wrapperInfoService.appendChild(infoService1);

            const infoService2 = document.createElement('div');
            infoService2.classList.add('info-service');
            infoService2.innerHTML = `
              <p><span class="selection-secondary">Тип услуги:</span> ${ServTypeName}</p>
              <p><span class="selection-secondary">Цена:</span> ${PriceServValue}</p>
              <p><span class="selection-secondary">Длительность:</span> ${ServDuration}</p>
            `;
            wrapperInfoService.appendChild(infoService2);


            //Скрытие ненужных материалов
            var tableDestMaterials = document.querySelector('#table_dest_material');

            // Итерируемся по строкам таблицы, пропуская заголовок
            for (var i = 1; i < tableDestMaterials.rows.length; i++) {
                var rowTableDestMaterials = tableDestMaterials.rows[i];

                if (parseInt(rowTableDestMaterials.cells[0].textContent) === parseInt(ServId)) {
                    rowTableDestMaterials.style.display = "";
                }
                else {
                    rowTableDestMaterials.style.display = "none";
                }
            }

            break;
        }
    }

    hideSelectedSourseMaterials();
}

//Функция которая скрывает необходимые материалы в таблице table_source_material
function hideSelectedSourseMaterials() {
    //Показать все материалы
    var tSM = document.querySelector('#table_source_material');
    for (var i = 1; i < tSM.rows.length; i++) {
        var rowTSM = tSM.rows[i];
        rowTSM.cells[0] = "False";
        rowTSM.style.display = "";
    }

    var tableDestMaterials = document.querySelector('#table_dest_material');
    var rowsTableDestMaterials = tableDestMaterials.getElementsByTagName('tr');
    var visibleRows = [];

    for (var i = 1; i < rowsTableDestMaterials.length; i++) {
        if (rowsTableDestMaterials[i].style.display !== 'none') {
            visibleRows.push(rowsTableDestMaterials[i]);
        }
    }

    for (var i = 0; i < visibleRows.length; i++) {

        //Извлечение MatId
        var cellMatId = visibleRows[i].cells[1];
        var inputElement = cellMatId.querySelector('input');
        var MatIdValue = inputElement.value;

        //Поиск MatIdValue в table_source_material и скрытие
        var tableSourceMaterials = document.querySelector('#table_source_material');
        for (var j = 1; j < tableSourceMaterials.rows.length; j++) {
            var rowTableSourceMaterials = tableSourceMaterials.rows[j];
            if (parseInt(rowTableSourceMaterials.cells[3].textContent) === parseInt(MatIdValue)) {
                rowTableSourceMaterials.cells[0] = "True";
                rowTableSourceMaterials.style.display = "none";
                break;
            }
        }

    }
}

//Обработчик клика на кнопке "add-material"
function handleButtonAddMaterial(event) {
    const row = event.target.closest('tr');

    if (!row) return; // Добавьте проверку, если строка не найдена


    const MatId = row.cells[3].textContent;
    const MatName = row.cells[4].textContent;
    const PriceMatValue = row.cells[6].textContent;

    row.cells[0].textContent = "True";
    row.style.display = "none";

    // Создание новой строки в таблице table_dest_material
    const tableDest = document.getElementById('table_dest_material');
    const newRow = document.createElement('tr');

    newRow.innerHTML = `
        <tr>
	        <td class="col-id-hidden">${globalSelectedServId}</td>

	        <td class="col-id-hidden">
		        <input hidden="" type="number" value="${MatId}" data-val="true" name="inputMedicalRecordModel.SelectedServices[*].SelectedMaterials[*].MatId">
	        </td>

	        <td>${MatName}</td>

	        <td>
                <input type="number" step="0.1" min="1" value="1" data-val="true" name="inputMedicalRecordModel.SelectedServices[*].SelectedMaterials[*].Count">
                <input name="__Invariant" type="hidden" value="inputMedicalRecordModel.SelectedServices[*].SelectedMaterials[*].Count">
            </td>

	        <td>${PriceMatValue}</td>

	        <td>${PriceMatValue}</td>
	        <td class="th-td-last">
		        <div class="btn-wrapper-table">
			        <a class="btn btn-wrapper-table-box del-material">
				        <img src="/image/icon-delete.png" width="16px">
			        </a>
		        </div>
	        </td>
        </tr>
    `;

    //Навешивание обработчика события на кнопку "del-service" в новой строке
    const deleteButtonDel = newRow.querySelector('.del-material');
    deleteButtonDel.addEventListener('click', handleDeleteMaterial);

    tableDest.querySelector('tbody').appendChild(newRow);
    attachEventListenersChangeInputCountMaterial();
    updateNamesServiceMaterials();
    calculateAll();
}

//Обработчик клика на кнопке "del-material"
function handleDeleteMaterial(event) {
    const row = event.target.closest('tr');

    if (!row) return; // Добавьте проверку, если строка не найдена

    //Извлечение MatId
    var cellMatId = row.cells[1];
    var inputElement = cellMatId.querySelector('input');
    var MatId = inputElement.value;

    //Поиск в таблице всех услуг необходимой услуги (которая удаляется)
    var tableSourse = document.querySelector('#table_source_material');

    // Итерируемся по строкам таблицы, пропуская заголовок
    for (var i = 1; i < tableSourse.rows.length; i++) {
        var rowTableSourse = tableSourse.rows[i];

        if (parseInt(rowTableSourse.cells[3].textContent) === parseInt(MatId)) {
            rowTableSourse.cells[0].textContent = "False";
            rowTableSourse.style.display = "";

            row.remove();

            break;
        }
    }
    updateNamesServiceMaterials();
    calculateAll();
}

window.addEventListener('load', function () {
    //Наложение обработчика события на выпадающие списки фильтров для сервисов
    document.getElementById("doctFilter").addEventListener("change", handleChangeSelectFiltersServices);
    document.getElementById("grpFilter").addEventListener("change", handleChangeSelectFiltersServices);

    //Наложение обработчика события на кнопки "add-service"
    const addServiceButtons = document.querySelectorAll('.add-service');
    addServiceButtons.forEach(button => {
        button.addEventListener('click', handleButtonAddService);
    });

    //Наложение обработчика события на кнопки "del-service"
    const deleteServiceButtons = document.querySelectorAll('.del-service');
    deleteServiceButtons.forEach(button => {
        button.addEventListener('click', handleDeleteService);
    });

    //Наложение обработчика события для кнопки "show-materials"
    const showButtons = document.querySelectorAll('.show-materials');
    showButtons.forEach(button => {
        button.addEventListener('click', handleShowMaterial);
    });

    //Наложение обработчика события на кнопки "add-material"
    const addMaterialButtons = document.querySelectorAll('.add-material');
    addMaterialButtons.forEach(button => {
        button.addEventListener('click', handleButtonAddMaterial);
    });

    //Наложение обработчика события для кнопки "del-material"
    const deleteMaterialButtons = document.querySelectorAll('.del-material');
    deleteMaterialButtons.forEach(button => {
        button.addEventListener('click', handleDeleteMaterial);
    });

    //Наложение обработчика события на кнопку "hide-block-materials"
    const hideBlockMaterialsButton = document.getElementById("hide-block-materials");
    hideBlockMaterialsButton.addEventListener('click', function () {
        var hiddenMaterialBlock = document.getElementById("hidden-material-block");
        hiddenMaterialBlock.style.display = "none";
        globalSelectedServId = null;
    });

    attachEventListenersChangeInputCountService();
    attachEventListenersChangeInputCountMaterial();
    calculateAll();
})
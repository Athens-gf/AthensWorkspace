// noinspection JSUnresolvedReference

let preJudgeParams = {};
let hiddenSkills = [];

db.open().then(_ => {
    getAmulets('hidden-skills', []).then((skills) => {
        hiddenSkills = skills;
        hideSkills();
    });
    getAmulets('having', []).then((havings) => $.each(havings, (_, params) => setAmuletRow(params)));
});

function hideSkills() {
    $.each(hiddenSkills, (_, skillId) => {
        let $input = $(`.hidden-skill[data-skill='${skillId}'] input`);
        $input.prop('checked', true);
        $input.parent().find('label').addClass('text-decoration-line-through');
        $(`.skill[data-skill='${skillId}']`).addClass('d-none');
    });
}

function resetSkill(serial) {
    let $select = $(`#select-skill-${serial}`);
    $select.prop('disabled', serial === 3);
    $select.children('img').attr('src', '');
    $select.children('span.skill-name').text('未設定');
    $select.children('span.level-value').text('');
    $select.removeData('skill');
    $select.removeData('level');

    $(`.level.serial${serial}`).addClass('d-none');
    $(`#skill-select-${serial}`).prop('disabled', true);
    $(`.skill${serial} input:checked`).prop('checked', false);
    $(`.level.serial${serial} input:checked`).prop('checked', false);
    $(`.skill${serial}`).removeClass('d-none');
    let $allGroup = $(`.group.serial${serial}`);
    $allGroup.removeClass('text-decoration-line-through');
    $allGroup.removeClass('btn-secondary');
    $allGroup.addClass('btn-outline-base');
    $allGroup.each((_, e) => $(e).addClass(`btn-group${$(e).data('group')}`));
    $allGroup.find('.predicted').addClass('d-none');
}

function resetParams() {
    $('.rare input:checked').prop('checked', false);
    $('.slots input:checked').prop('checked', false);
    $('.slots').removeClass('d-none');
    for (let serial = 1; serial <= 3; serial++) {
        resetSkill(serial);
    }
    $addButton.prop('disabled', true);
}

function setAmuletRow(params) {
    let $row = $('#base-has-amulet').clone().removeAttr('id');
    $row.appendTo('#has-amulet-list');
    $row.removeClass('d-none');
    $row.data('id', params.id);
    $row.find('.has-rare').each(function () {
        if ($(this).data('rare') !== params.rare) {
            $(this).remove();
        }
    });
    $row.find('.has-slot').each(function () {
        if ($(this).data('val') !== params.slots) {
            $(this).remove();
        }
    });
    for (let index = 0; index < 4; index++) {
        let skill = params.skills[index];
        let $target = $row.find(`.has-skill${index + 1}`);
        if (skill === undefined) {
            $target.empty();
            continue;
        }
        let icon = $(`.hidden-skill[data-name='${skill.name}']`).data('icon');
        let src = $(`#skill-icon-${icon}`).attr('src');
        $target.find('.name').text(skill.name);
        $target.find('.level').text(`Lv${skill.level}`);
        $target.find('img').attr('src', src);
    }
}

function checkCanAdd() {
    let $rare = $(".rare input:checked");
    let $rareInput = $rare.parent();
    let $slots = $('.slots input:checked');
    let $slotsInput = $slots.parent();
    let $select1 = $('#select-skill-1');
    let $select2 = $('#select-skill-2');
    let $select3 = $('#select-skill-3');
    let canAdd = $rareInput.length === 1 && $slotsInput.length === 1 && $select1.data('level') && $select2.data('level') && $select3.data('level');
    $addButton.prop('disabled', !canAdd);
}

function judgeAmulet() {
    let $select1 = $("#select-skill-1");
    let $select2 = $("#select-skill-2");
    let $select3 = $("#select-skill-3");
    let params = {
        rawRare: $('.rare input:checked').parent().data('rare'),
        rawSlots: $('.slots input:checked').parent().data('val'),
        skillId1: $select1.data('skill'),
        level1: $select1.data('level'),
        skillId2: $select2.data('skill'),
        level2: $select2.data('level'),
        skillId3: $select3.data('skill'),
        level3: $select3.data('level'),
    };
    if (params === preJudgeParams) return;
    preJudgeParams = params;
    $.post('JudgeAmulet', params, function (data) {
        console.log(data)
        if (data.rare)
            $(`#rare${data.rare}`).prop('checked', true);
        else
            $('.rare input:checked').prop('checked', false);

        if (data.slots)
            $(`#slots${data.slots}`).prop('checked', true);
        else
            $('.slots input:checked').prop('checked', false);
        $('.slots').addClass('d-none');
        $('.skill').addClass('d-none');
        $.each(data.resetSkillSerials, (_, serial) => resetSkill(serial));
        $.each(data.selectableSlots, (_, slots) => $(`#slots${slots}`).parent().removeClass('d-none'));
        $.each(data.selectableGroupIdsDic, (serial, groupIds) => {
            let $allGroup = $(`.group.serial${serial}`);
            $allGroup.addClass('text-decoration-line-through');
            $allGroup.addClass('btn-secondary');
            $allGroup.removeClass('btn-outline-base');
            $allGroup.each((_, e) => $(e).removeClass(`btn-group${$(e).data('group')}`));
            $.each(groupIds, (_, groupId) => {
                let $group = $(`.group.serial${serial}.group${groupId}`);
                $group.removeClass('text-decoration-line-through');
                $group.removeClass('btn-secondary');
                $group.addClass('btn-outline-base');
                $group.each((_, e) => $(e).addClass(`btn-group${groupId}`));
                $(`.skill${serial}.group${groupId}`).removeClass('d-none');
            });
        });
        hideSkills();
        $('.group').find('.predicted').addClass('d-none');
        $.each(data.predictedGroupIdsDic, (serial, groupIds) => {
            $.each(groupIds, (_, groupId) => {
                $(`.serial${serial}.group${groupId}`).find('.predicted').removeClass('d-none');
            })
        });
        let $select3 = $('#select-skill-3');
        if ($('.group.serial3.btn-outline-base').length === 0) {
            $select3.prop('disabled', true);
            $select3.children('span.skill-name').text('設定不可');
            $select3.data('skill', undefined);
            $select3.data('level', -1);
        } else  {
            let $select1 = $('#select-skill-1');
            let $select2 = $('#select-skill-2');
            let skillId1 = $select1.data('skill');
            let skillId2 = $select2.data('skill');
            if ($select3.data('level') === undefined){
                $select3.children('span.skill-name').text('未設定');
                $select3.data('skill', undefined);
                $select3.data('level', undefined);
            }
            let canSelectSkill3 = skillId1 && skillId2;
            $select3.prop('disabled', !canSelectSkill3);
        }
        for (let serial = 1; serial <= 3; serial++) {
            let $selectSkill = $(`#select-skill-${serial}`);
            let skillId = $selectSkill.data('skill');
            if (skillId) {
                $(`.skill[data-skill='${skillId}']:not([data-serial='${serial}'])`).addClass('d-none');
            }
        }
        checkCanAdd();
    });
}

$('.rare input').change(() => judgeAmulet());
$('.slots input').change(() => judgeAmulet());

function changeSkill(serial, skillId) {
    let $select1 = $("#select-skill-1");
    let $select2 = $("#select-skill-2");
    let $select3 = $("#select-skill-3");
    let params = {
        rawRare: $('.rare input:checked').parent().data('rare'),
        rawSlots: $('.slots input:checked').parent().data('val'),
        skillId1: $select1.data('skill'),
        level1: $select1.data('level'),
        skillId2: $select2.data('skill'),
        level2: $select2.data('level'),
        skillId3: $select3.data('skill'),
        level3: $select3.data('level'),
    };
    params[`skillId${serial}`] = skillId;
    params[`level${serial}`] = undefined;

    $.post('GuessLevels', params, function (data) {
        $(`.level.serial${serial}`).addClass('d-none');
        $.each(data, (_, level) => $(`#skill${serial}-level${level}`).parent().removeClass('d-none'));
        let $canSelectLevels = $(`.level.serial${serial}:not(.d-none)`);
        if ($canSelectLevels.length === 1) {
            $canSelectLevels.children('input').prop('checked', true).trigger('change');
        } else {
            $(`.level.serial${serial}.d-none input`).prop('checked', false);
            if ($(`.level.serial${serial} input:checked`).length !== 1)
                $(`#skill-select-${serial}`).prop('disabled', true);
        }
    });
}

$('.skill input').change(function () {
    changeSkill($(this).parent().data('serial'), $(this).parent().data('skill'));
});

$(`.level input`).change(function () {
    $(`#skill-select-${$(this).parent().data('serial')}`).prop('disabled', false);
});

$(`button.skill-select`).click(function () {
    let serial = $(this).data('serial');
    let $skill = $(`.skill${serial} input:checked`).parent();
    let $label = $skill.find('label');
    let $level = $(`.level.serial${serial} input:checked`).parent();
    let $button = $(`#select-skill-${serial}`);
    let level = $level.data('level');
    $button.find('img').attr('src', $label.find('img').attr('src'));
    $button.find('.skill-name').text($label.find('span').text());
    $button.find('.level-value').text(`Lv${level}`);
    let skillId = $skill.data('skill');
    $button.data('skill', skillId);
    $button.data('level', level);
    $('.modal').modal('hide');
    judgeAmulet();
});

let $addButton = $('#add-amulet');

$addButton.on('click', () => {
    let $rare = $(".rare input:checked");
    let $rareInput = $rare.parent();
    let $slots = $('.slots input:checked');
    let $slotsInput = $slots.parent();
    let $select1 = $('#select-skill-1');
    let $select2 = $('#select-skill-2');
    let $select3 = $('#select-skill-3');
    if ($rareInput.length !== 1 || $slotsInput.length !== 1 || !$select1.data('skill') || !$select2.data('skill') || !$select3.data('skill')) return;
    let params = {
        id: self.crypto.randomUUID(),
        rare: $rareInput.data('rare'), 
        slots: $slotsInput.data('val'),
        skills: [{
            name: $select1.find('.skill-name').text(), level: $select1.data('level'),
        }, {
            name: $select2.find('.skill-name').text(), level: $select2.data('level'),
        }],
    };
    let level3 = $select3.data('level');
    if (level3) {
        params.skills[2] = {
            name: $select3.find('.skill-name').text(), level: level3,
        }
    }
    setAmuletRow(params);
    getAmulets('having', []).then((havings) => {
        havings.push(params);
        setAmulets('having', havings);
    });
    resetParams();
    $addButton.prop('disabled', true);
});

$('#reset-amulet-params').click(() => resetParams());

$('.hidden-skill input').on('change', function() {
    let $target = $(this).parent();
    let skillId = $target.data('skill');
    let flag = $(this).prop('checked');
    if (flag) {
        $target.find('label').addClass('text-decoration-line-through');
        hiddenSkills.push(skillId);
        hiddenSkills = [...new Set(hiddenSkills)].sort((a, b) => a - b);
    } else {
        $target.find('label').removeClass('text-decoration-line-through');
        hiddenSkills = hiddenSkills.filter(val => val !== skillId);
    }
    setAmulets('hidden-skills', hiddenSkills);
    hideSkills();
});

function removeAmulet(button) {
    let $target = $(button).closest('.has-amulet');
    let id = $target.data('id');
    $target.remove();
    getAmulets('having', []).then((havings) => {
        havings = havings.filter(params => params.id !== id);
        setAmulets('having', havings);
    });
}

function exportAmulets() {
    getAmulets('having', []).then((havings) => {
        let output = '';
        havings.forEach(params => {
            console.log(params)
            let text = '';
            for (let i = 0; i < 3; i++) {
                let skill = params.skills[i];
                if (skill) {
                    text += `${skill.name},${skill.level},`;
                } else {
                    text += ',0,';
                }
            }
            output += text + params.slots + '\n';
        });
        $('#import-area').val(output);
    });
}

function importAmulets(isAdditional) {
    let input = $('#import-area').val();
    if (input === '') return;

    $.post('ImportAmulets', {input: input}, function (data) {
        $.each(data, function (_, params) {
            console.log(params)
        })
    });
}

function allRemoveAmulets() {
    setAmulets('having', []);
    $('#has-amulet-list').empty();
}

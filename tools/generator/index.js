import fs from 'fs';

var content = fs.readFileSync('input.txt', 'utf8');
var lines = content.split('\n');

var prefixedTypes = lines.map((item, index) => {
    if (item.trim().startsWith('public var')) {
        var name = /public var (\w+): (\[?\w+\]?\??)/.exec(item)[1];
        var type = /public var (\w+): (\[?\w+\]?\??)/.exec(item)[2];

        return [name, type.replace(/\[|\]|\?/img, ''), type.indexOf('?') > -1, type.indexOf('[') > -1];
    }

    return '';
});

const swift2CsTypeMapping = {
    'Double': 'double',
    'Bool': 'bool',
    'String': 'string',
    'StyleColor': 'Color'
};

annnotationManager_swiftProperties2JavaProperties();

function annnotationManager_swiftProperties2JavaProperties() {
    var transformed = prefixedTypes
        .filter(x => !!x)
        .map((item) => {
            let propName = item[0];
            let propType = item[1];

            if (swift2CsTypeMapping[propType]) {
                propType = swift2CsTypeMapping[propType];
            }

            let nullable = item[2] && !item[3] ? '?' : '';
            let array = item[3] ? '[]' : '';
            let cstype = `${propType}${nullable}${array}`;
            let csname = `${propName.substring(0,1).toUpperCase() + propName.substring(1)}`;
            return `    public ${cstype} ${csname} 
        {
            get => nativeManager.${csname};
            set => nativeManager.${csname} = value;
        }`;
        });
    
    fs.writeFileSync('output.txt', transformed.join('\n'));
}

function swiftProperties2CsInterfaceProperties() {
    var transformed = prefixedTypes
        .filter(x => !!x)
        .map((item) => {
            let propName = item[0];
            let propType = item[1];

            if (swift2CsTypeMapping[propType]) {
                propType = swift2CsTypeMapping[propType];
            }

            let nullable = item[2] && !item[3] ? '?' : '';
            let array = item[3] ? '[]' : '';
            let cstype = `${propType}${nullable}${array}`;
            let csname = `${propName.substring(0,1).toUpperCase() + propName.substring(1)}`;
            return `${cstype} ${csname} { get; set; }`;
        });
    
    fs.writeFileSync('output.txt', transformed.join('\n'));
}

function swiftProperties2CsClassProperties() {
    var transformed = lines.map((item, index) => {
        let infoIndex = -1;
        if (/^\s+public var/.test(item)) {
            infoIndex = 0;
        }
        else if (/^\s+return/.test(item)) { 
            infoIndex = 2;
        } 
        else if (/^\s+layerProperties/.test(item)) { 
            infoIndex = 5;
        }
        let propType;
        let propName;
        let nullable;
        let array;
        let cstype;
        let csname;

        if (infoIndex > -1) {
            let info = prefixedTypes[index-infoIndex];
            propName = info[0];
            propType = info[1];

            if (swift2CsTypeMapping[propType]) {
                propType = swift2CsTypeMapping[propType];
            }

            nullable = info[2] && !info[3] ? '?' : '';
            array = info[3] ? '[]' : '';
            cstype = `${propType}${nullable}${array}`;
            csname = `${propName.substring(0,1).toUpperCase() + propName.substring(1)}`;
        }

        if (/^\s+public var/.test(item)) {
            return `    public ${cstype} ${csname}
        {`;
        }
        
        if (/^\s+return/.test(item)) {
            return `            return GetProperty<${cstype}>("${/"([^"]+)"/.exec(item)[1]}", default);`;
        }

        if (/^\s+layerProperties/.test(item)) {
            return `            SetProperty("${/"([^"]+)"/.exec(item)[1]}", value);`;
        }
        
        return item;
    })
    
    fs.writeFileSync('output.txt', transformed.join('\n'));
}
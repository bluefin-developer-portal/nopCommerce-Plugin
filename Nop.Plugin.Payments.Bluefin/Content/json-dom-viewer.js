/*
 * (c) 2022-2023 Voxgig, Aleksandar Milenkovic
 * MIT License
 * https://github.com/BeAllAround/json-dom-viewer
*/

const SPACE = '&nbsp;',
    DOWN_TRIANGLE = '&#x25BC;',
    RIGHT_TRIANGLE = '&#x25B6;'

function applyStyle(elem, style) {
    Object.assign(elem.style, style)
}

function type(obj) {
    if (!obj) { // JSON null
        return false
    }
    return obj.constructor
}

function newType(constructor) {
    let style = {
        'number': { color: '#746eed' },
        'string': { color: '#039376' },
        'boolean': { color: '#746eed' },
    }
    return style[constructor]
}

function simpleDiv(content = '', style, typing) {
    let el, html
    el = document.createElement('div')
    applyStyle(el, { display: 'inline', })
    if (typeof content == 'string' && typing) {
        el.innerHTML += '"'
    }

    // value content
    if (content === null) { // null - special case
        el.innerHTML = 'null'
    } else {
        el.innerHTML += content.toString()
    }

    if (typeof content == 'string' && typing) {
        el.innerHTML += '"'
    }
    if (style) {
        applyStyle(el, style)
    }

    if (content === null) { // null - special case
        applyStyle(el, { color: 'gray', })
    }
    /*
    return Object.create({ $el: el, addTo(elem){
                       elem.appendChild(this.$el)
                     },
    })
    */
    return el
}

function valueDiv(value, typing) {
    let style = newType(typeof value)
    return simpleDiv(value, style, typing)
}

function create(obj, dotted = false, spacing = 0, path, cache) {
    let el, html, tab, opening, closing
    let b
    el = document.createElement('div')
    el.id = 'dict'
    applyStyle(el, { color: 'black', })
    applyStyle(el, { display: 'inline', })
    // el.innerHTML = `${JSON.stringify(obj)}`
    tab = SPACE.repeat(spacing)

    el.appendChild(opening = simpleDiv('{'))

    opening.addEventListener('mouseover', (event) => { // highligher
        applyStyle(opening, { color: 'red' })
        applyStyle(closing, { color: 'red' })
    })
    opening.addEventListener('mouseleave', (event) => {
        applyStyle(opening, { color: 'black' })
        applyStyle(closing, { color: 'black' })
    })

    let keys = Object.keys(obj), key
    for (let i in keys) {
        let keyDom, valueDom

        b = true
        key = keys[i]
        // console.log([key, obj[key]])
        el.appendChild(document.createElement('br'))
        el.appendChild(simpleDiv(tab))
        el.appendChild(keyDom = simpleDiv(key.toString(), { color: '#5db0d7', }))
        el.appendChild(simpleDiv(':'))
        el.appendChild(simpleDiv(SPACE.repeat(2)))
        // el.appendChild(simpleDiv(key.toString() + ': '))

        // console.log(keyDom.style)
        // console.log(keyDom)


        // :hover keyDom
        keyDom.addEventListener('mouseover', (event) => {
            event.stopPropagation()
            applyStyle(keyDom, {
                opacity: 0.5, 'background-color': 'black',
                padding: '1px', border: '1px solid black', 'border-radius': '6px',
            })
        })
        keyDom.addEventListener('mouseleave', (event) => {
            event.stopPropagation()
            applyStyle(keyDom, {
                opacity: '', 'background-color': '',
                padding: '', border: '', 'border-radius': '',
            }) // reset style
        })

        keyDom.addEventListener('click', event => {
            if (!keyDom.enabled) {
                applyStyle(valueDom, {
                    'background-color': '#29323d',
                    padding: '0px', border: '1px inset #5db0d7', 'border-radius': '6px',
                })
                /*
                let _popup = document.createElement('div')
                    _popup.textContent = path
                    _popup.id = 'popup'
                    applyStyle(_popup, {'position': 'absolute',
                                        'top': '200px',
                                        'right': '200px',
                    })
                    keyDom.appendChild(_popup)
            */
                keyDom.enabled = true
            } else {
                applyStyle(valueDom, {
                    'background-color': '',
                    padding: '', border: '', 'border-radius': '',
                })
                keyDom.enabled = false
                /*
            for(let child of keyDom.children) {
                          if(child.id == 'popup'){
                                  child.remove()
                          }
                    }
            */
            }
        })

        if (type(obj[key]) === Object || type(obj[key]) == Array) {
            if (Object.keys(obj[key]).length != 0) {
                el.appendChild(valueDom = viewJSON(obj[key], spacing + 6, path + '.' + key, cache))
            }
            else { // empty Object => {}
                el.appendChild(valueDom = simpleDiv("{}"))
            }
        } else if (obj[key] === null) {
            el.appendChild(valueDom = valueDiv(null, true))
        } else {
            el.appendChild(valueDom = valueDiv(obj[key], true))
        }

        if (keys.length - i - 1) {
            el.appendChild(simpleDiv(','))
        }

        /*
        // :hover valueDom - TBD
                valueDom.addEventListener('mouseover', (event) => {
          applyStyle(valueDom, {'background-color': '#29323d',
              padding: '0px', border: '1px inset #5db0d7', 'border-radius': '6px',})
                })
                valueDom.addEventListener('mouseleave', (event) => {
          applyStyle(valueDom, {'background-color': '',
              padding: '', border: '', 'border-radius': '',})
                })
        */
    }

    if (dotted) {
        el.appendChild(simpleDiv('...'))
    }
    if (keys.length != 0) {
        el.appendChild(document.createElement('br')) // '\n'
        tab = SPACE.repeat(spacing - 6)
        // ( spacing - indent ) => correct indent
        // console.log('tab: ', {tab, path, spacing} )
        el.appendChild(closing = simpleDiv(tab + '}'))
    } else {
        el.appendChild(closing = simpleDiv('}'))
    }

    closing.addEventListener('mouseover', (event) => {
        applyStyle(opening, { color: 'red' })
        applyStyle(closing, { color: 'red' })
    })
    closing.addEventListener('mouseleave', (event) => {
        applyStyle(opening, { color: 'black' })
        applyStyle(closing, { color: 'black' })
    })

    return el
}

function icon(obj, dict, spacing, path, cache) {
    let el;
    let rendered;
    el = document.createElement('div');
    el.innerHTML = RIGHT_TRIANGLE;
    applyStyle(el, { color: 'gray', 'font-size': '12px' }); // default style
    (function _open() {
        let found_path = cache[path]
        if (found_path) {
            obj[0].remove()
            if (!obj[0].enabled) {
                el.innerHTML = DOWN_TRIANGLE
                applyStyle(el, { color: 'gray' }) // update style
                obj[0] = create(dict, false, spacing, path, cache)
                applyStyle(obj[0], { display: "inline" })
                obj[0].enabled = true
            } else {
                applyStyle(el, { color: 'gray' }) // update style
                el.innerHTML = RIGHT_TRIANGLE
                obj[0] = create({}, true, 0, path, cache)
                obj[0].enabled = false
            }
            // console.log(obj[0])
        }

    })()
    el.addEventListener('click', e => {
        // console.log('e: ', e)
        // console.log('path: ', path)
        e.stopPropagation()
        obj[0].remove()
        if (!obj[0].enabled) {
            el.innerHTML = DOWN_TRIANGLE
            applyStyle(el, { color: 'gray' }) // update style
            obj[0] = create(dict, false, spacing, path, cache)
            applyStyle(obj[0], { display: "inline" })
            obj[0].enabled = true

            cache[path] = true


        } else {
            applyStyle(el, { color: 'gray' }) // update style
            el.innerHTML = RIGHT_TRIANGLE
            obj[0] = create({}, true, 0, path, cache)
            obj[0].enabled = false

            // delete all the paths starting with that path - this will help close child objects once a main parent object closes
            for (let _path in cache) {
                if (_path.startsWith(path + '.') || _path === path) { // '.b1' startsWith '.b' bug
                    delete cache[_path]
                }
            }
            // delete cache[path]

        }

        // console.log('cache: ', cache)
        // console.log('retrievedObject: ', JSON.parse(retrievedObject));

        // console.log('obj: ', obj[0]) // for debugging
        el.parentNode.appendChild(obj[0]) // similar to linked list - prev, next
    })
    applyStyle(el, { display: 'inline', })
    return el
}

function viewJSON(dict, spacing = 2, path = '', cache = {}) {
    let obj = [create({}, true, 0)] // default
    let div = document.createElement('div')
    applyStyle(div, { display: 'inline', })
    div.appendChild(icon(obj, dict, spacing, path, cache))
    div.appendChild(obj[0])
    return div
}

function viewJSONAsString(json_string) { // obsolete
    return viewJSON(JSON.parse(json_string))
}
const tileSize = 32
const levelSize = {
    width: 18,
    height: 12,
}
const actionPalette = {
    SPECIAL: '#a300ff',
    MOVE_LEFT: '#00ff0e',
    MOVE_RIGHT: '#00c2ff',
    JUMP: '#000000',
    DO_NOTHING: '#004eff',
}

let table

function preload() {
    table = loadTable('../../../Logs/GA log 2023-12-22-T-10-58-34 - level 3 - population 100.csv', 'csv', 'header')
}

function setup() {
    createCanvas(tileSize * levelSize.width, tileSize * levelSize.height)

    colorMode(HSB, 360, 100, 100)

    const sampleRows = table.findRows('cellSize', 'componentField')
    renderArchive(sampleRows)
    for (const row of sampleRows) {
        const trajectories = JSON.parse(row.get('terminalTrajectories'))
        if (trajectories.length === 0) {
            continue
        }
        renderRow(trajectories)
    }
}

function renderArchive(sampleRows) {
    const colCells = new Map()
    for (const row of sampleRows) {
        const archive = JSON.parse(row.get('archive'))
        for (const cell of archive) {
            if (colCells.has(`${cell.x}-${cell.y}`)) {
                const existingCell = colCells.get(`${cell.x}-${cell.y}`)
                existingCell.timesSeen += cell.timesSeen
                existingCell.timesChosen += cell.timesChosen
            } else {
                colCells.set(`${cell.x}-${cell.y}`, cell)
            }
        }
    }

    let maxSeenCount = 0
    let maxChosenCount = 0
    for (const cell of colCells.values()) {
        if (cell.timesSeen > maxSeenCount) {
            maxSeenCount = cell.timesSeen
        }
        if (cell.timesChosen > maxChosenCount) {
            maxChosenCount = cell.timesChosen
        }
    }

    stroke('#ccc')
    noFill()
    for (let x = 0; x < levelSize.width; x++) {
        for (let y = 0; y < levelSize.height; y++) {
            const cellFromArchive = colCells.get(`${x}-${y}`)
            if (cellFromArchive) {
                fill(lerpColor(color(60, 100, 100), color(0, 100, 100), cellFromArchive.timesSeen / maxSeenCount))
            } else {
                fill(0, 0, 100)
            }
            rect(x * tileSize, height - ((y + 1) * tileSize), tileSize, tileSize)
        }
    }
}

function renderRow(trajectories) {
    for (const trajectory of trajectories) {
        if (trajectory.length < 2) {
            continue
        }
        noStroke()
        const firstCell = trajectory[0]
        fill(actionPalette[firstCell.action])
        ellipse(
            ...tilePos(firstCell),
            tileSize / 2,
            tileSize / 2
        )
        const lastCell = trajectory[trajectory.length - 1]
        fill(actionPalette[lastCell.action])
        ellipse(
            ...tilePos(lastCell),
            tileSize / 2,
            tileSize / 2
        )

        noStroke()
        fill(actionPalette[firstCell.action])
        text(firstCell.action, ...tilePos(firstCell))
        noFill()
        strokeWeight(4)
        strokeJoin(ROUND)
        let lastAction = firstCell.action
        for (let index = 0; index < trajectory.length - 1; index++) {
            const cell = trajectory[index]
            if (cell.action !== lastAction) {
                noStroke()
                fill(actionPalette[cell.action])
                textAlign(LEFT, BASELINE)
                text(cell.action, ...tilePos(cell))
                noFill()
                stroke(actionPalette[cell.action])
            }
            line(...tilePos(trajectory[index]), ...tilePos(trajectory[index + 1]))
            lastAction = cell.action
        }
    }
}

function tilePos(cell, isCenterPosition = true) {
    return [
        cell.x * tileSize + (isCenterPosition ? tileSize / 2 : 0),
        height - (cell.y * tileSize + (isCenterPosition ? tileSize / 2 : 0)),
    ]
}

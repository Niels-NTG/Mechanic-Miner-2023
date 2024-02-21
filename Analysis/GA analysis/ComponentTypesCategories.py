import glob

import pandas as pd
import matplotlib.pyplot as plt


def getTableFilesInFolder(path: str) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames: list[pd.DataFrame] = []
    for file in files:
        frame = pd.read_csv(file)
        # where filter causes filtered rows to be replaced with NaN
        frame = frame.where(frame['fitness'] > 0)
        frame['filename'] = file.split('/')[-1]
        frame['TGM'] = (
            frame['gameObject'].astype(str) + ',' +
            frame['component'].astype(str) + ',' +
            frame['componentField'].astype(str) + ',' +
            frame['modifier'].astype(str)
        )
        frames.append(frame)
    mergedFrames = pd.concat(frames, ignore_index=True)
    return mergedFrames


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    allComponentTypes = tables.dropna().drop_duplicates(subset=['component'])['component'].to_list()
    populationDiversityTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'totalUniqueGenes',
            *allComponentTypes
        ]
    )
    for name, group in groupedData:
        totalUniqueGeneCount = group['TGM'].agg(['nunique'])['nunique']
        uniqueComponentCount = group.drop_duplicates(subset=['TGM']).value_counts(subset=['component'])
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'totalUniqueGenes': totalUniqueGeneCount,
        })
        for componentName, componentTypeCount in uniqueComponentCount.items():
            newRow[componentName] = componentTypeCount
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=1, ncols=4, figsize=(18, 6))
    makePlot(3, 'Wall', populationDiversityTable, allComponentTypes, 0, axes)
    makePlot(4, 'Wall + Elevation', populationDiversityTable, allComponentTypes, 1, axes)
    makePlot(5, 'Ceiling', populationDiversityTable, allComponentTypes, 2, axes)
    makePlot(6, 'Chasm', populationDiversityTable, allComponentTypes, 3, axes)


def makePlot(level: int, levelName: str, table: pd.DataFrame, componentTypes: list, x: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='area',
        y=componentTypes,
        x='generation',
        ax=axes[x],
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 50)
    plot.set_xlabel('')
    if x != 3:
        plot.get_legend().remove()


diversityTables = getTableFilesInFolder('./data/f65acba/')
runAnalysis(diversityTables)

plt.tight_layout()
plt.show()

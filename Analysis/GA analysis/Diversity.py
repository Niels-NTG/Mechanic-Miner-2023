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
    populationDiversityTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'median unique genes count',
            'unique genes count 5%',
            'unique genes count 25%',
            'unique genes count 75%',
            'unique genes count 95%',
            'median non-zero fitness population size',
            'population size 5%',
            'population size 25%',
            'population size 75%',
            'population size 95%',
            # 'totalUniqueGenes',
            'fitness mean',
            'fitness 5%',
            'fitness 25%',
            'fitness 75%',
            'fitness 95%',
        ]
    )
    for name, group in groupedData:
        # totalUniqueGeneCount = group['TGM'].agg(['nunique'])['nunique']
        uniqueGeneCount = group.groupby(['filename'])['TGM'].agg(['nunique'])
        uniqueGeneCountPercentiles = uniqueGeneCount.describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
        uniqueGeneCountMedian = uniqueGeneCount.agg(['median'])
        populationCount = group.groupby(['filename'])['TGM'].agg(['count'])
        populationCountPercentiles = populationCount.describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
        populationCountMedian = populationCount.agg(['median'])
        meanFitness = group['fitness'].agg(['mean', 'std'])
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'median unique genes count': uniqueGeneCountMedian['nunique']['median'],
            'unique genes count 5%': uniqueGeneCountPercentiles['nunique']['5%'],
            'unique genes count 25%': uniqueGeneCountPercentiles['nunique']['25%'],
            'unique genes count 75%': uniqueGeneCountPercentiles['nunique']['75%'],
            'unique genes count 95%': uniqueGeneCountPercentiles['nunique']['95%'],
            'median non-zero fitness population size': populationCountMedian['count']['median'],
            'population size 5%': populationCountPercentiles['count']['5%'],
            'population size 25%': populationCountPercentiles['count']['25%'],
            'population size 75%': populationCountPercentiles['count']['75%'],
            'population size 95%': populationCountPercentiles['count']['95%'],
            # 'totalUniqueGenes': totalUniqueGeneCount,
            'fitness mean': meanFitness['mean'],
            'fitness std': meanFitness['std'],
        })
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=3, ncols=4, figsize=(18, 14))
    makePlot(3, 'Wall', populationDiversityTable, 0, axes)
    makePlot(4, 'Wall + Elevation', populationDiversityTable, 1, axes)
    makePlot(5, 'Ceiling', populationDiversityTable, 2, axes)
    makePlot(6, 'Chasm', populationDiversityTable, 3, axes)


def makePlot(level: int, levelName: str, table: pd.DataFrame, x: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['median unique genes count'],
        x='generation',
        ax=axes[0, x],
        color='g',
    )
    plot.fill_between(
        table['generation'],
        table['unique genes count 5%'],
        table['unique genes count 95%'],
        alpha=0.2,
        color='g',
    )
    plot.fill_between(
        table['generation'],
        table['unique genes count 25%'],
        table['unique genes count 75%'],
        alpha=0.4,
        color='g',
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
    plot.set_ylim(1, 16)
    plot.set_xlabel('')
    if x != 0:
        plot.get_legend().remove()

    plot2 = table.plot(
        kind='line',
        y=['fitness mean'],
        x='generation',
        ax=axes[1, x],
        color='orange',
    )
    plot2.fill_between(
        table['generation'],
        table['fitness mean'] - table['fitness std'],
        table['fitness mean'] + table['fitness std'],
        alpha=0.2,
        color='orange',
    )
    plot2.set_xlim(1, 15)
    plot2.set_ylim(0, 1)
    plot2.set_xlabel('')
    if x != 0:
        plot2.get_legend().remove()

    plot3 = table.plot(
        kind='line',
        y=['median non-zero fitness population size'],
        x='generation',
        ax=axes[2, x],
        color='purple',
    )
    plot3.fill_between(
        table['generation'],
        table['population size 5%'],
        table['population size 95%'],
        alpha=0.2,
        color='purple',
    )
    plot3.fill_between(
        table['generation'],
        table['population size 25%'],
        table['population size 75%'],
        alpha=0.4,
        color='purple',
    )
    plot3.set_xlim(1, 15)
    plot3.set_ylim(0, 100)
    if x != 0:
        plot3.get_legend().remove()


diversityTables = getTableFilesInFolder('./data/f65acba/')
runAnalysis(diversityTables)

plt.tight_layout()
plt.show()

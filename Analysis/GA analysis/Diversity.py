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
            'fitness median',
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
        fitnessMedian = group['fitness'].agg(['median'])
        fitnessPercentiles = group['fitness'].describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
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
            'fitness median': fitnessMedian['median'],
            'fitness 5%': fitnessPercentiles['5%'],
            'fitness 25%': fitnessPercentiles['25%'],
            'fitness 75%': fitnessPercentiles['75%'],
            'fitness 95%': fitnessPercentiles['95%'],
        })
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

    fig1, medianFitnessAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianFitnessPlot(3, 'Wall', populationDiversityTable, 0, 0, medianFitnessAxes)
    makeMedianFitnessPlot(4, 'Wall + Elevation', populationDiversityTable, 1, 0, medianFitnessAxes)
    makeMedianFitnessPlot(5, 'Ceiling', populationDiversityTable, 2, 0, medianFitnessAxes)
    makeMedianFitnessPlot(6, 'Deadly River', populationDiversityTable, 0, 1, medianFitnessAxes)
    makeMedianFitnessPlot(8, 'Ravine', populationDiversityTable, 1, 1, medianFitnessAxes)
    makeMedianFitnessPlot(9, 'Ravine + Spikes', populationDiversityTable, 2, 1, medianFitnessAxes)
    plt.tight_layout()
    plt.show()

    fig2, medianUniqueGeneCountAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianUniqueGeneCountPlot(3, 'Wall', populationDiversityTable, 0, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(4, 'Wall + Elevation', populationDiversityTable, 1, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(5, 'Ceiling', populationDiversityTable, 2, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(6, 'Deadly River', populationDiversityTable, 0, 1, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(8, 'Ravine', populationDiversityTable, 1, 1, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(9, 'Ravine + Spikes', populationDiversityTable, 2, 1, medianUniqueGeneCountAxes)
    plt.tight_layout()
    plt.show()

    fig3, medianNonZeroFitnessPopulationCountAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianNonZeroFitnessPopulationCount(3, 'Wall', populationDiversityTable, 0, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCount(4, 'Wall + Elevation', populationDiversityTable, 1, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCount(5, 'Ceiling', populationDiversityTable, 2, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCount(6, 'Deadly River', populationDiversityTable, 0, 1, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCount(8, 'Ravine', populationDiversityTable, 1, 1, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCount(9, 'Ravine + Spikes', populationDiversityTable, 2, 1, medianNonZeroFitnessPopulationCountAxes)
    plt.tight_layout()
    plt.show()



def makeMedianFitnessPlot(level: int, levelName: str, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['fitness median'],
        x='generation',
        ax=axes[y, x],
        color='darkorange',
    )
    plot.fill_between(
        table['generation'],
        table['fitness 5%'],
        table['fitness 95%'],
        alpha=0.2,
        color='darkorange',
    )
    plot.fill_between(
        table['generation'],
        table['fitness 25%'],
        table['fitness 75%'],
        alpha=0.4,
        color='darkorange',
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 1)
    plot.set_xlabel('')
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


def makeMedianUniqueGeneCountPlot(level: int, levelName: str, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['median unique genes count'],
        x='generation',
        ax=axes[y, x],
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
    plot.set_ylim(0, 14)
    plot.set_xlabel('')
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


def makeMedianNonZeroFitnessPopulationCount(level: int, levelName: str, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['median non-zero fitness population size'],
        x='generation',
        ax=axes[y, x],
        color='purple',
    )
    plot.fill_between(
        table['generation'],
        table['population size 5%'],
        table['population size 95%'],
        alpha=0.2,
        color='purple',
    )
    plot.fill_between(
        table['generation'],
        table['population size 25%'],
        table['population size 75%'],
        alpha=0.4,
        color='purple',
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 100)
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


diversityTables = getTableFilesInFolder('./data/f65acba/')
runAnalysis(diversityTables)

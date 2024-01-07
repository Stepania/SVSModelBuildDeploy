import datetime as dt
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import MathsUtilities as MUte
import matplotlib.dates as mdates

ResidueTestNames = ['StovN0.5High.csv','StovN1.0High.csv','StovN1.5High.csv','StovN3.0High.csv','StovN4.5High.csv',
                    'StovN0.5Low.csv','StovN1.0Low.csv','StovN1.5Low.csv','StovN3.0Low.csv','StovN4.5Low.csv']
cols = ['k','b','g','y','r','k','b','g','y','r']
lines = ['-','-','-','-','-','--','--','--','--','--']

ResidueTests = []
for t in ResidueTestNames:
    rt = pd.read_csv('C:\\GitHubRepos\\SVSModelBuildDeploy\\TestModel\\TestConsole\\bin\\Debug\\net6.0\\OutputFiles\\'+t,index_col="Date",date_format='%d/%m/%Y %H:%M:%S %p')
    ResidueTests.append(rt)
AllResidueTests = pd.concat(ResidueTests,axis=0,keys=ResidueTestNames)

Graph = plt.figure()
ax = Graph.add_subplot(1,1,1)
pos = 0
for t in ResidueTestNames:
    plt.plot(AllResidueTests.loc[t,'ResidueN'].cumsum(),lines[pos],color=cols[pos],label = t[:-4])
    pos +=1
plt.legend(loc=(1.1,0.2))
plt.ylabel('Cum Net Residue mineralisation (kg/ha)')

plt.show()

#!/usr/bin/env python
#-*- coding: utf-8 -*-

from __future__ import division
from collections import defaultdict
from math import log
import os

def prepare(filename):
	stop_symbols = '.,!?:;-\n\r()0123456789'
	with open(filename) as f:
		tokens = f.read().split()
		c = filename[6]
		results = [t.strip(stop_symbols) for t in tokens if t.decode('utf8')[0].isupper()]
		results = [t for t in results if len(t.decode('utf8'))>2]
	return results, c

def tokens(filename):
	stop_symbols = '.,!?:;-\n\r()0123456789'
	with open(filename) as f:
		tokens = f.read().split()
		results = [t.strip(stop_symbols) for t in tokens if t.decode('utf8')[0].isupper()]
		results = [t for t in results if len(t.decode('utf8'))>2]
	return results

def tmpfile(filename, data, c):
	f = open(filename, 'a')
	for d in data:
		f.write('{0} {1}\n'.format(d, c))
	

def train(samples):
    classes, freq = defaultdict(lambda:0), defaultdict(lambda:0)
    for feats, label in samples:
        classes[label] += 1

        freq[label, feats] += 1

    for label, feat in freq:
        freq[label, feat] /= classes[label]
    for c in classes:
        classes[c] /= len(samples)

    return classes, freq

def process(f1, f2):
	r, c = prepare(f1)
	tmpfile(f2, r,c)

def classify_b(classifier, feats):
	classes, prob = classifier
	rescl = 0;
	ressum = 1000;
	for cl in classes:
		s = 0
		count1=0
		count3=0
		for l, f in prob:
			if l is cl:
				count3+=1
				if f.encode('utf8') in feats:
					s += log(prob[cl,f])
					count1 +=1
				else:
					s += log(1-prob[cl,f])
		print cl, s, count1
		if s < ressum:
			ressum = s
			rescl = cl
	print rescl

def classify_m(classifier, feats):
	classes, prob = classifier
	rescl = 0;
	ressum = 1000;
	for cl in classes:
		s = 0
		count1=0
		count3=0
		for f in feats:
			if not prob[cl , f.decode('utf8')] is 0:
				s += log(prob[cl,f.decode('utf8')])
				count1 +=1
				
		print cl, s, count1
		if s < ressum:
			ressum = s
			rescl = cl
	print rescl

def get_features(sample): return (sample[-1],) # get last letter

process('train/p1.txt', 'tmp.txt')
process('train/p2.txt', 'tmp.txt')
process('train/p3.txt', 'tmp.txt')
process('train/r1.txt', 'tmp.txt')
process('train/r2.txt', 'tmp.txt')
process('train/r3.txt', 'tmp.txt')
process('train/r4.txt', 'tmp.txt')

samples = (line.decode('utf-8').split() for line in open('tmp.txt'))
features = [(feat, label) for feat, label in samples]
classifier = train(features)

t = tokens('test1.txt')
#t = tokens('train/p1.txt')
classify_b(classifier, t)
classify_m(classifier, t)

os.remove('tmp.txt')

#print 'gender: ', classify(classifier, get_features(u'ыва'))

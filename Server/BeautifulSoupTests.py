# -*- coding: utf-8 -*-
"""Unit tests for Beautiful Soup.

These tests make sure the Beautiful Soup works as it should. If you
find a bug in Beautiful Soup, the best way to express it is as a test
case like this that fails."""

import unittest
from BeautifulSoup import *

class SoupTest(unittest.TestCase):

    def assertSoupEquals(self, toParse, rep=None, c=BeautifulSoup):
        """Parse the given text and make sure its string rep is the other
        given text."""
        if rep == None:
            rep = toParse
        self.assertEqual(str(c(toParse)), rep)


class FollowThatTag(SoupTest):

    "Tests the various ways of fetching tags from a soup."

    def setUp(self):
        ml = """
        <a id="x">1</a>
        <A id="a">2</a>
        <b id="b">3</a>
        <b href="foo" id="x">4</a>
        <ac width=100>4</ac>"""
        self.soup = BeautifulStoneSoup(ml)

    def testFindAllByName(self):
        matching = self.soup('a')
        self.assertEqual(len(matching), 2)
        self.assertEqual(matching
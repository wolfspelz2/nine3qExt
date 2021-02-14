import * as $ from 'jquery';
import { as } from '../lib/as';
import { IObserver, IObservable } from '../lib/ObservableProperty';
import { ContentApp } from './ContentApp';
import { Participant } from './Participant';
import { Config } from '../lib/Config';
import { Utils } from '../lib/Utils';
import { Environment } from '../lib/Environment';
import { Menu, MenuColumn, MenuItem, MenuHasIcon, MenuOnClickClose, MenuHasCheckbox } from './Menu';
import { listenerCount } from 'process';

export class PointsGenerator
{
    constructor(private base: number, private fullLevels: number, private fractionalLevels: number) { }

    getPartsList(digits: Array<{ exp: number, count: number }>): Array<string>
    {
        let list = [];

        let count = 0;
        let lastExp = -1;
        for (let i = 0; i < digits.length; i++) {
            let digit = digits[i];
            let gap = (lastExp < 0 ? digit.exp + 1 : lastExp) - digit.exp;
            count += gap;
            if (count > this.fullLevels + this.fractionalLevels) { break; }
            if (count > this.fullLevels && count <= this.fullLevels + this.fractionalLevels) {
                if (lastExp > 0 && lastExp - digit.exp > this.fractionalLevels + 1) { break; }
                list.push('' + digit.exp + '-' + digit.count);
            } else {
                if (lastExp > 0 && lastExp - digit.exp > 1) { break; }
                for (let j = 0; j < digit.count; j++) {
                    list.push('' + digit.exp);
                }
            }
            lastExp = digit.exp;
        }

        return list;
    }

    getDigitList(nPoints: number): Array<{ exp: number, count: number }>
    {
        let list = [];

        let work = nPoints;
        let position = this.largestDigit(work);
        let count = 0;
        while (position >= 0) {
            let fraction = Math.pow(this.base, position);
            while (work - fraction >= 0) {
                work -= fraction;
                count++;
            }
            if (count > 0) {
                list.push({ exp: position, count: count });
            }
            position--;
            count = 0;
        }

        return list;
    }

    largestDigit(n: number): number
    {
        if (n < 1) { return 0; }
        if (n == 1) { return 0; }
        let exp = 0;
        let cap = 1;
        while (n >= cap) {
            exp += 1;
            cap = Math.pow(this.base, exp);
        }
        return exp - 1;
    }

    /*
// ============================================================================
//  Copyright (C) zweitgeist GmbH
// ============================================================================

#if !defined POINTGENERATOR_H
#define POINTGENERATOR_H

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "../stdafx.h"

#include "SElem.h"
#include "SListT.h"
#include "SLog.h"

class PointListElem: public SElem
{
public:
  PointListElem()
  {}

  PointListElem(PSTR szValue)
  :sPoints_(szValue)
  {}

  virtual ~PointListElem()
  {}

  // set:
  void Points(PSTR szValue) { sPoints_ = szValue; }

  // get:
  SString Points() { return sPoints_; }

protected:
  // mind:
  SString sPoints_;

};

class PointList: public SListT<PointListElem, SElem>
{
public:
  PointList::~PointList();
  int Add(PSTR szPoints);
  int AddFirst(PSTR szPoints);
  int AddLast(PSTR szPoints);
  int Delete(PointListElem*& e);
  int DeleteLast();

protected:
  typedef SListT<PointListElem, SElem> mother;

};

//=============================================================================

class PointGenerator
{
public :
  PointGenerator();
  virtual ~PointGenerator();

  int Generate(int nPoints);
  PointList& GetPointList() { return slPointList_; }

  void SetBase(int nValue) { nBase_ = nValue; }
  void SetDigits(int nValue) { nDigits_ = nValue; }
  void SetLevelRange(int nValue) { nMaxLevels_ = nValue; }

  int GetBase() { return nBase_; }
  int GetDigits() { return nDigits_; }
  int GetLevelRange() { return nMaxLevels_; }

protected:
  int nBase_;
  int nDigits_;
  int nMaxLevels_;
  PointList slPointList_;


#ifdef UNITTEST
public:
  static int UnitTestBegin();
  static int UnitTestRun();
  static int UnitTestEnd();

#endif

};

#endif

//=============================================================================

// ============================================================================
//  Copyright (C) zweitgeist
// ============================================================================

#include "stdafx.h"
#include "SString.h"
#include "PointGenerator.h"

#ifdef UNITTEST
#include "UnitTest.h"
#endif

PointGenerator::PointGenerator()
:nBase_(5)
,nDigits_(4)
,nMaxLevels_(0)
{
}

PointGenerator::~PointGenerator()
{
}

int PointGenerator::Generate(int nPoints)
{
  int ok = 1;

  int nWorkPoints = nPoints;

  int nHighestLevel = 0;
  int nPartLevel = 0;

  int bDone = 0;
  for (int nDigit=0; ((nWorkPoints > 0) && (nDigit<nDigits_)); ++nDigit) {

    int bMatch=0;
    for (int nLevel=0; (nWorkPoints > 0 && !bMatch); ++nLevel) {

      // determine level
      int nValue = (int) pow(nBase_, nLevel);

      if (nWorkPoints == nValue) {
        bMatch = 1;
      } else if (nWorkPoints < nValue && nLevel > 0) {
        --nLevel;
        nValue = (int) pow(nBase_, nLevel);
        bMatch = 1;
      }

      if (bMatch) {
        nWorkPoints -= nValue;
        nHighestLevel = (nLevel>nHighestLevel?nLevel:nHighestLevel);
        SString sPoints;

        if (nMaxLevels_ > 0) {
          // we only want to see the highest n levels
          if ((nLevel == nHighestLevel - nMaxLevels_) || (nLevel == 0 && nHighestLevel - nMaxLevels_ < 0)) {
            // certain levels are displayed partly
            ++nPartLevel;
            if (nWorkPoints < (int) pow(nBase_, nLevel)) {
              sPoints.strcatf("%d_%d", nLevel, nPartLevel);
            } else {
              --nDigit;
            }
          } else if (nLevel > nHighestLevel - nMaxLevels_) {
            // full levels are full
            sPoints.strcatf("%d", nLevel);
          } else {
            // levels which we don't want to see anymore
            bDone = 1;
          }
        } else {
          // we want to see all levels
          ++nPartLevel;
          if (nLevel == 0 && nWorkPoints <= 0) {
            // display points below 1 partly
            sPoints.strcatf("%d_%d", nLevel, nPartLevel);
          } else if (nLevel > 0) {
            // all other are full
            sPoints.strcatf("%d", nLevel);
            --nPartLevel;
          }
        }

        if (sPoints != "") {
          slPointList_.AddLast(sPoints);
        }

      }

    } // match to power

    if (bDone) { break; }

  } // digits

  return ok;
}

//== PointList ================================================================

PointList::~PointList()
{
  for (PointListElem* e=0; e = First(); ) {
    Delete(e);
  }
}

int PointList::Add(PSTR szPoints)
{
  int ok = 0;

  PointListElem* e = new PointListElem(szPoints);
  if (e != 0) {
    ok = mother::Add(e);
  } else {
    SLOG((SLOG_ERROR, "PointList::Add() new PointListElem failed\n"));
  }

  return ok;
}

int PointList::AddFirst(PSTR szPoints)
{
  int ok = 0;

  PointListElem* e = new PointListElem(szPoints);
  if (e != 0) {
    ok = mother::AddFirst(e);
  } else {
    SLOG((SLOG_ERROR, "PointList::AddFirst() new PointListElem failed\n"));
  }

  return ok;
}

int PointList::AddLast(PSTR szPoints)
{
  int ok = 0;

  PointListElem* e = new PointListElem(szPoints);
  if (e != 0) {
    ok = mother::AddLast(e);
  } else {
    SLOG((SLOG_ERROR, "PointList::AddLast() new PointListElem failed\n"));
  }

  return ok;
}

int PointList::Delete(PointListElem*& e)
{
  int ok = 1;

  ok = Rmv(e);
  if (ok) {
    delete e;
    e = 0;
  } else {
    SLOG((SLOG_ERROR, "PointList::Delete() Rmv() failed\n"));
  }

  return ok;
}

int PointList::DeleteLast()
{
  int ok = 1;
  PointListElem* e = Last();
  if (e != 0) {
    ok = Rmv(e);
    if (ok) {
      delete e;
      e = 0;
    } else {
      SLOG((SLOG_ERROR, "PointList::DeleteLast() Rmv() failed\n"));
    }
  }

  return ok;
}

//== UnitTest =================================================================

#ifdef UNITTEST

static SString g_sErrorMessage;


static int PointGenerator_VisibleRange()
{
  int ok = 1;

  PointGenerator oGen;

  int nBase   = oGen.GetBase();
  int nDigits = oGen.GetDigits();
  int nMax    = nDigits * int(pow(nBase, nDigits));

  for (int i=0; i<=nMax&&ok; ++i) {
    oGen.GetPointList().Empty();
    ok = oGen.Generate(i);
    if (!ok) {
      g_sErrorMessage.strcatf("oGen.Generate(%d) failed", i);
    }

    if (ok) {
      PointList slPointList = oGen.GetPointList();
      int nListCount = slPointList.count();
      if (nListCount > nDigits) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == %d, nDigits == %d", i, nListCount, nDigits);
        ok = 0;
      } else if (i > 0 && nListCount == 0) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == 0", i);
        ok = 0;
      }
      if (ok) {
        for (PointListElem* e=0; (e=slPointList.Next(e)); ) {
        }
      }
    }

  }

  return ok;
}

static int PointGenerator_HigherRange()
{
  int ok = 1;

  PointGenerator oGen;

  int nBase   = oGen.GetBase();
  int nDigits = oGen.GetDigits();
  int nMax    = nDigits * int(pow(nBase, nDigits));

  for (int i=nMax; i<=(2*nMax)&&ok; ++i) {
    oGen.GetPointList().Empty();
    ok = oGen.Generate(i);
    if (!ok) {
      g_sErrorMessage.strcatf("oGen.Generate(%d) failed", i);
    }

    if (ok) {
      PointList slPointList = oGen.GetPointList();
      int nListCount = slPointList.count();
      if (nListCount > nDigits) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == %d, nDigits == %d", i, nListCount, nDigits);
        ok = 0;
      } else if (i > 0 && nListCount == 0) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == 0", i);
        ok = 0;
      }
      if (ok) {
        for (PointListElem* e=0; (e=slPointList.Next(e)); ) {
        }
      }
    }

  }

  return ok;
}

static int PointGenerator_StrangeRange()
{
  int ok = 1;

  PointGenerator oGen;

  int nBase   = oGen.GetBase();
  int nDigits = oGen.GetDigits();
  int nMax    = -( nDigits * int(pow(nBase, nDigits)) );

  for (int i=0; i>nMax&&ok; --i) {
    oGen.GetPointList().Empty();
    ok = oGen.Generate(i);
    if (!ok) {
      g_sErrorMessage.strcatf("oGen.Generate(%d) failed", i);
    }

    if (ok) {
      PointList slPointList = oGen.GetPointList();
      int nListCount = slPointList.count();
      if (nListCount > 0) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == %d", i, nListCount);
        ok = 0;
      } else if (i > 0 && nListCount == 0) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == 0", i);
        ok = 0;
      } else if (nListCount > nDigits) {
        g_sErrorMessage.strcatf("Points == %d && slPointList.count() == %d, nDigits == %d", i, nListCount, nDigits);
        ok = 0;
      }
      if (ok) {
        for (PointListElem* e=0; (e=slPointList.Next(e)); ) {
        }
      }
    }

  }

  return ok;
}

static int PointGenerator_GeneralTest(int nPoints, PSTR szExpected, int nMaxLevels=0, int nBase=5)
{
  int ok = 1;

  SList slResults;
  KeyValueBlob2SList(szExpected, slResults, ",", "");
  int nDigits = slResults.count();

  PointGenerator oGen;
  oGen.SetBase(nBase);
  oGen.SetDigits(nDigits);
  oGen.SetLevelRange(nMaxLevels);

  ok = oGen.Generate(nPoints);
  if (!ok) {
    g_sErrorMessage.strcatf("oGen.Generate(%d) failed", nPoints);
  }

  if (ok) {
    PointList slPointList = oGen.GetPointList();
    int nListCount = slPointList.count();
    if (nListCount == 0 && nPoints == 0) {
      g_sErrorMessage.strcatf("slPointList.count() == %d, nPoints == %d", nListCount, nPoints);
      ok = 0;
    } else if (nListCount > nDigits) {
      g_sErrorMessage.strcatf("slPointList.count() == %d, nDigits == %d", nListCount, nDigits);
      ok = 0;
    }

    int i=1;
    PointListElem* ePt=0;
    for (SElem* eRes=0; (eRes=slResults.Next(eRes))&&ok; ++i) {
      SString sResPoints = eRes->Name();

      if ( ePt = slPointList.Next(ePt) ) {
        if (sResPoints != SString(ePt->Points())) {
          g_sErrorMessage.strcatf("Result: %s  Expected: %s", (char*) ePt->Points(), (char*) sResPoints);
          ok = 0;
        }
      }
    }

  } // ok

  if (!ok && g_sErrorMessage != "") {
    g_sErrorMessage.strcatf("\r\n  Params: nPoints == %d, szExpected == \"%s\", nBase == %d, nDigits == %d", nPoints, szExpected, nBase, nDigits);
  }

  return ok;
}

static int PointGenerator_ExplicitlyNumber()
{
  int ok = 1;

  if (ok) { ok = PointGenerator_GeneralTest(1,    "0_1"); }
  if (ok) { ok = PointGenerator_GeneralTest(2,    "0_2"); }
  if (ok) { ok = PointGenerator_GeneralTest(3,    "0_3"); }
  if (ok) { ok = PointGenerator_GeneralTest(4,    "0_4"); }
  if (ok) { ok = PointGenerator_GeneralTest(6,    "1,0_1"); }

  if (ok) { ok = PointGenerator_GeneralTest(5,    "1"); }
  if (ok) { ok = PointGenerator_GeneralTest(25,   "2"); }
  if (ok) { ok = PointGenerator_GeneralTest(125,  "3"); }
  if (ok) { ok = PointGenerator_GeneralTest(625,  "4"); }
  if (ok) { ok = PointGenerator_GeneralTest(3125, "5"); }
  if (ok) { ok = PointGenerator_GeneralTest(15625,"6"); }

  if (ok) { ok = PointGenerator_GeneralTest(32,   "2,1,0_2"); }
  if (ok) { ok = PointGenerator_GeneralTest(82,   "2,2,2,1,0_2"); }
  if (ok) { ok = PointGenerator_GeneralTest(783,  "4,3,2,1,0_3"); }
  if (ok) { ok = PointGenerator_GeneralTest(783,  "4,3,2,1"); }
  if (ok) { ok = PointGenerator_GeneralTest(812,  "4,3,2,2"); }

  if (ok) { ok = PointGenerator_GeneralTest(812,  "4,3,2_2", 2); }
  if (ok) { ok = PointGenerator_GeneralTest(11,   "1,1,0_3", 2, 4); }
  if (ok) { ok = PointGenerator_GeneralTest(7,    "1,0_3", 2, 4); }
  if (ok) { ok = PointGenerator_GeneralTest(1007, "4,4,4,3,3,3,2_2", 2, 4); }
  if (ok) { ok = PointGenerator_GeneralTest(1008, "4,4,4,3,3,3,2_3", 2, 4); }

  if (ok) { ok = PointGenerator_GeneralTest(2500, "4,4,4,4"); }
  if (ok) { ok = PointGenerator_GeneralTest(3905, "5,4,3,2,1"); }

  return ok;
}


int PointGenerator::UnitTestBegin()
{
  int ok = 1;

  UnitTest::Instance().Register("PointGenerator_VisibleRange");
  UnitTest::Instance().Register("PointGenerator_HigherRange");
  UnitTest::Instance().Register("PointGenerator_StrangeRange");
  UnitTest::Instance().Register("PointGenerator_ExplicitlyNumber");

  return ok;
}

int PointGenerator::UnitTestRun()
{
  int ok = 1;

  if (ok) { ok = PointGenerator_VisibleRange(); UnitTest::Instance().Complete("PointGenerator_VisibleRange", ok, g_sErrorMessage); }
  if (ok) { ok = PointGenerator_HigherRange(); UnitTest::Instance().Complete("PointGenerator_HigherRange", ok, g_sErrorMessage); }
  if (ok) { ok = PointGenerator_StrangeRange(); UnitTest::Instance().Complete("PointGenerator_StrangeRange", ok, g_sErrorMessage); }
  if (ok) { ok = PointGenerator_ExplicitlyNumber(); UnitTest::Instance().Complete("PointGenerator_ExplicitlyNumber", ok, g_sErrorMessage); }

  return ok;
}

int PointGenerator::UnitTestEnd()
{
  int ok = 1;
  return ok;
}


#endif

    */
}

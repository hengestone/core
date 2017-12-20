﻿// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2014 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

namespace WebSharper.Sitelets.Tests

open WebSharper
open WebSharper.Testing
open WebSharper.Sitelets
open PerformanceTests
open RouterOperators

[<JavaScript>]
module ClientServerTests =

    let ShiftedRouter = 
        Router.Shift "perf-tests" <| Router.Infer<Action>()

    let Tests apiBaseUri =
        TestCategory "Sitelets Client-server routing" {
            Test "compatibility tests" {
                let! testValuesAndServerLinks = GetTestValues()
                let testValuesAndClientLinks =
                    testValuesAndServerLinks |> Array.map (fun (testValue, _) ->
                        testValue, ShiftedRouter.Link testValue    
                    )
                equal testValuesAndServerLinks testValuesAndClientLinks
            }

            Test "Ajax test" {
                let! testValuesAndServerLinks = GetTestValues()
                let ajaxResults = ResizeArray()
                let! ajaxResults =
                    async {
                        let arr = ResizeArray()
                        for testValue, _ in testValuesAndServerLinks do
                            try
                                do! Expect testValue
                                let! res = Router.Ajax ShiftedRouter testValue
                                arr.Add (box res)
                            with e ->
                                arr.Add (box e.StackTrace)
                        return arr.ToArray()
                    }
                let expectedResults =
                    testValuesAndServerLinks |> Array.map (fun (_, serverLink) ->
                        box serverLink
                    )
                deepEqual ajaxResults expectedResults
            }

            Test "Primitive tests" {
                let rUnit = rRoot |> Router.MapTo ()
                equal (Router.Link rUnit ()) "/"
                equal (Router.Parse rUnit (Route.FromUrl "/")) (Some ())
                equal (Router.Link rInt 2) "/2"
                equal (Router.Parse rInt (Route.FromUrl "/2")) (Some 2)
            }

            Test "Combinator tests" {
                let! testValuesAndServerLinks = CombinatorTests.GetTestValues()
                let testValuesAndClientLinks =
                    testValuesAndServerLinks |> Array.map (fun (testValue, _) ->
                        testValue, CombinatorTests.constructed.Link testValue    
                    )
                equal testValuesAndServerLinks testValuesAndClientLinks
            }
        }

    let RunTests apiBaseUri =
        Runner.RunTests [|
            Tests apiBaseUri
        |]

